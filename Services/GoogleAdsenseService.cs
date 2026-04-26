using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PortfolioSite.Data;
using PortfolioSite.Models.Adsense;
using PortfolioSite.Models.Options;

namespace PortfolioSite.Services;

public sealed class GoogleAdsenseService
{
    private const int ConnectionRecordId = 1;
    private const string ReadOnlyScope = "https://www.googleapis.com/auth/adsense.readonly";
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string TokenRevocationEndpoint = "https://oauth2.googleapis.com/revoke";
    private const string AccountsEndpoint = "https://adsense.googleapis.com/v2/accounts";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly PortfolioDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleAdsenseService> _logger;
    private readonly GoogleAdsenseOptions _options;
    private readonly IDataProtector _refreshTokenProtector;

    public GoogleAdsenseService(
        PortfolioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<GoogleAdsenseOptions> optionsAccessor,
        ILogger<GoogleAdsenseService> logger
    )
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = optionsAccessor.Value;
        _refreshTokenProtector = dataProtectionProvider.CreateProtector("PortfolioSite.GoogleAdsense.RefreshToken.v1");
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ClientId)
        && !string.IsNullOrWhiteSpace(_options.ClientSecret);

    public string BuildAuthorizationUrl(string redirectUri, string state)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Google AdSense OAuth クライアントが未設定です。");
        }

        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = ReadOnlyScope,
            ["access_type"] = "offline",
            ["include_granted_scopes"] = "true",
            ["prompt"] = "consent",
            ["state"] = state
        };

        return QueryHelpers.AddQueryString(AuthorizationEndpoint, parameters);
    }

    public async Task ConnectAsync(
        string authorizationCode,
        string redirectUri,
        CancellationToken cancellationToken = default
    )
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Google AdSense OAuth クライアントが未設定です。");
        }

        var tokenResponse = await ExchangeAuthorizationCodeAsync(authorizationCode, redirectUri, cancellationToken);
        var refreshToken = tokenResponse.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException(
                "Google から refresh token を取得できませんでした。Google 側の同意画面でアクセスを許可したうえで再接続してください。");
        }

        var accounts = await ListAccountsAsync(tokenResponse.AccessToken, cancellationToken);
        var account = accounts.FirstOrDefault();
        if (account is null || string.IsNullOrWhiteSpace(account.Name))
        {
            throw new InvalidOperationException("AdSense アカウントを取得できませんでした。Google 側で AdSense が有効か確認してください。");
        }

        var record = await _dbContext.Set<AdminAdsenseConnectionRecord>()
            .SingleOrDefaultAsync(item => item.Id == ConnectionRecordId, cancellationToken);

        var now = DateTime.UtcNow;
        if (record is null)
        {
            record = new AdminAdsenseConnectionRecord
            {
                Id = ConnectionRecordId,
                ConnectedAtUtc = now
            };
            _dbContext.Add(record);
        }

        record.AccountName = account.Name;
        record.AccountDisplayName = string.IsNullOrWhiteSpace(account.DisplayName)
            ? account.Name
            : account.DisplayName;
        record.RefreshTokenCiphertext = _refreshTokenProtector.Protect(refreshToken);
        record.UpdatedAtUtc = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<AdminAdsenseConnectionRecord>()
            .SingleOrDefaultAsync(item => item.Id == ConnectionRecordId, cancellationToken);

        if (record is null)
        {
            return;
        }

        var refreshToken = TryUnprotect(record.RefreshTokenCiphertext);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await TryRevokeRefreshTokenAsync(refreshToken, cancellationToken);
        }

        _dbContext.Remove(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GoogleAdsenseDashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = false
            };
        }

        var record = await _dbContext.Set<AdminAdsenseConnectionRecord>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == ConnectionRecordId, cancellationToken);

        if (record is null)
        {
            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = true
            };
        }

        var connectionSnapshot = new GoogleAdsenseConnectionSnapshot
        {
            AccountName = record.AccountName,
            AccountDisplayName = record.AccountDisplayName,
            ConnectedAtUtc = record.ConnectedAtUtc
        };

        string refreshToken;
        try
        {
            refreshToken = _refreshTokenProtector.Unprotect(record.RefreshTokenCiphertext);
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "Failed to unprotect the stored AdSense refresh token.");
            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = true,
                Connection = connectionSnapshot,
                ErrorMessage = "保存済みの AdSense 接続情報を復号できませんでした。再接続してください。"
            };
        }

        try
        {
            var accessToken = await RefreshAccessTokenAsync(refreshToken, cancellationToken);

            var summaryTask = GenerateReportAsync(
                record.AccountName,
                accessToken,
                metrics: ["PAGE_VIEWS", "IMPRESSIONS", "CLICKS", "ESTIMATED_EARNINGS", "PAGE_VIEWS_RPM"],
                dimensions: [],
                dateRange: "LAST_30_DAYS",
                orderBy: [],
                limit: null,
                cancellationToken
            );

            var dailyTask = GenerateReportAsync(
                record.AccountName,
                accessToken,
                metrics: ["PAGE_VIEWS", "CLICKS", "ESTIMATED_EARNINGS"],
                dimensions: ["DATE"],
                dateRange: "LAST_7_DAYS",
                orderBy: ["+DATE"],
                limit: 7,
                cancellationToken
            );

            var sitesTask = GenerateReportAsync(
                record.AccountName,
                accessToken,
                metrics: ["PAGE_VIEWS", "CLICKS", "ESTIMATED_EARNINGS"],
                dimensions: ["OWNED_SITE_DOMAIN_NAME"],
                dateRange: "LAST_30_DAYS",
                orderBy: ["-PAGE_VIEWS"],
                limit: 5,
                cancellationToken
            );

            await Task.WhenAll(summaryTask, dailyTask, sitesTask);

            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = true,
                Connection = connectionSnapshot,
                Summary = MapSummary(summaryTask.Result),
                DailyMetrics = MapDailyMetrics(dailyTask.Result),
                TopSites = MapTopSites(sitesTask.Result)
            };
        }
        catch (GoogleAdsenseApiException ex)
        {
            _logger.LogWarning(ex, "Failed to load Google AdSense dashboard data for account {AccountName}.", record.AccountName);
            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = true,
                Connection = connectionSnapshot,
                ErrorMessage = MapDashboardError(ex)
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while loading Google AdSense dashboard data.");
            return new GoogleAdsenseDashboardSnapshot
            {
                IsConfigured = true,
                Connection = connectionSnapshot,
                ErrorMessage = "Google AdSense への接続に失敗しました。ネットワークと OAuth 設定を確認してください。"
            };
        }
    }

    private async Task<GoogleTokenResponse> ExchangeAuthorizationCodeAsync(
        string authorizationCode,
        string redirectUri,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = authorizationCode,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            })
        };

        return await SendTokenRequestAsync(request, cancellationToken);
    }

    private async Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            })
        };

        var response = await SendTokenRequestAsync(request, cancellationToken);
        return response.AccessToken;
    }

    private async Task<GoogleTokenResponse> SendTokenRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(JsonOptions, cancellationToken);
        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Google OAuth token response was empty.");
        }

        return tokenResponse;
    }

    private async Task<List<GoogleAdsenseAccountDto>> ListAccountsAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, AccountsEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleAdsenseAccountsResponse>(JsonOptions, cancellationToken);
        return payload?.Accounts ?? [];
    }

    private async Task<GoogleAdsenseReportResponse> GenerateReportAsync(
        string accountName,
        string accessToken,
        IReadOnlyList<string> metrics,
        IReadOnlyList<string> dimensions,
        string dateRange,
        IReadOnlyList<string> orderBy,
        int? limit,
        CancellationToken cancellationToken
    )
    {
        var queryBuilder = new QueryBuilder();
        foreach (var dimension in dimensions)
        {
            queryBuilder.Add("dimensions", dimension);
        }

        foreach (var metric in metrics)
        {
            queryBuilder.Add("metrics", metric);
        }

        foreach (var sortKey in orderBy)
        {
            queryBuilder.Add("orderBy", sortKey);
        }

        queryBuilder.Add("dateRange", dateRange);
        queryBuilder.Add("languageCode", "ja-JP");

        if (limit.HasValue)
        {
            queryBuilder.Add("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://adsense.googleapis.com/v2/{accountName}/reports:generate{queryBuilder.ToQueryString()}"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var client = CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleAdsenseReportResponse>(JsonOptions, cancellationToken);
        return payload ?? new GoogleAdsenseReportResponse();
    }

    private async Task TryRevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenRevocationEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["token"] = refreshToken
                })
            };

            using var client = CreateClient();
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Google token revocation returned {StatusCode}.", response.StatusCode);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogInformation(ex, "Failed to revoke Google AdSense refresh token.");
        }
    }

    private static GoogleAdsenseSummarySnapshot? MapSummary(GoogleAdsenseReportResponse report)
    {
        var columns = BuildColumnIndex(report);
        var totals = report.Totals;
        if (totals is null || totals.Cells.Count == 0)
        {
            return null;
        }

        return new GoogleAdsenseSummarySnapshot
        {
            PageViews = ParseLong(GetCellValue(totals, columns, "PAGE_VIEWS")),
            Impressions = ParseLong(GetCellValue(totals, columns, "IMPRESSIONS")),
            Clicks = ParseLong(GetCellValue(totals, columns, "CLICKS")),
            EstimatedEarnings = ParseDecimal(GetCellValue(totals, columns, "ESTIMATED_EARNINGS")),
            PageViewsRpm = ParseDecimal(GetCellValue(totals, columns, "PAGE_VIEWS_RPM")),
            CurrencyCode = report.Headers
                .FirstOrDefault(header => string.Equals(header.Name, "ESTIMATED_EARNINGS", StringComparison.Ordinal))?
                .CurrencyCode
        };
    }

    private static IReadOnlyList<GoogleAdsenseDailyMetric> MapDailyMetrics(GoogleAdsenseReportResponse report)
    {
        var columns = BuildColumnIndex(report);
        return report.Rows
            .Select(row => new GoogleAdsenseDailyMetric
            {
                DateLabel = GetCellValue(row, columns, "DATE") ?? "",
                PageViews = ParseLong(GetCellValue(row, columns, "PAGE_VIEWS")),
                Clicks = ParseLong(GetCellValue(row, columns, "CLICKS")),
                EstimatedEarnings = ParseDecimal(GetCellValue(row, columns, "ESTIMATED_EARNINGS"))
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.DateLabel))
            .ToList();
    }

    private static IReadOnlyList<GoogleAdsenseSiteMetric> MapTopSites(GoogleAdsenseReportResponse report)
    {
        var columns = BuildColumnIndex(report);
        return report.Rows
            .Select(row => new GoogleAdsenseSiteMetric
            {
                Domain = GetCellValue(row, columns, "OWNED_SITE_DOMAIN_NAME") ?? "",
                PageViews = ParseLong(GetCellValue(row, columns, "PAGE_VIEWS")),
                Clicks = ParseLong(GetCellValue(row, columns, "CLICKS")),
                EstimatedEarnings = ParseDecimal(GetCellValue(row, columns, "ESTIMATED_EARNINGS"))
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Domain))
            .ToList();
    }

    private static Dictionary<string, int> BuildColumnIndex(GoogleAdsenseReportResponse report)
    {
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var position = 0; position < report.Headers.Count; position++)
        {
            var name = report.Headers[position].Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                index[name] = position;
            }
        }

        return index;
    }

    private static string? GetCellValue(
        GoogleAdsenseReportRow row,
        IReadOnlyDictionary<string, int> columns,
        string columnName
    )
    {
        if (!columns.TryGetValue(columnName, out var index) || index < 0 || index >= row.Cells.Count)
        {
            return null;
        }

        return row.Cells[index].Value;
    }

    private static long ParseLong(string? value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0L;
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private string? TryUnprotect(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
        {
            return null;
        }

        try
        {
            return _refreshTokenProtector.Unprotect(ciphertext);
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "Failed to unprotect the stored AdSense refresh token during disconnect.");
            return null;
        }
    }

    private static string MapDashboardError(GoogleAdsenseApiException exception)
    {
        if (string.Equals(exception.ErrorCode, "invalid_grant", StringComparison.OrdinalIgnoreCase))
        {
            return "保存済みの Google 接続が失効しています。再接続してください。";
        }

        return exception.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Google AdSense リクエストが拒否されました。OAuth クライアント設定と redirect URI を確認してください。",
            HttpStatusCode.Unauthorized => "Google AdSense の認証に失敗しました。再接続してください。",
            HttpStatusCode.Forbidden => "AdSense データの読み取り権限が不足しています。Google 側の権限を確認してください。",
            HttpStatusCode.TooManyRequests => "Google AdSense API の呼び出し回数上限に達しました。少し待ってから再読み込みしてください。",
            _ => "Google AdSense データの取得に失敗しました。設定と接続状態を確認してください。"
        };
    }

    private static async Task<GoogleAdsenseApiException> CreateApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var errorCode = response.StatusCode.ToString();
        var message = $"Google API returned {(int)response.StatusCode} {response.StatusCode}.";

        if (!string.IsNullOrWhiteSpace(payload))
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;

                if (root.TryGetProperty("error", out var errorProperty))
                {
                    if (errorProperty.ValueKind == JsonValueKind.String)
                    {
                        errorCode = errorProperty.GetString() ?? errorCode;
                    }
                    else if (errorProperty.ValueKind == JsonValueKind.Object)
                    {
                        if (errorProperty.TryGetProperty("status", out var statusProperty))
                        {
                            errorCode = statusProperty.GetString() ?? errorCode;
                        }

                        if (errorProperty.TryGetProperty("message", out var messageProperty))
                        {
                            message = messageProperty.GetString() ?? message;
                        }
                    }
                }

                if (root.TryGetProperty("error_description", out var descriptionProperty))
                {
                    message = descriptionProperty.GetString() ?? message;
                }
            }
            catch (JsonException)
            {
                message = payload;
            }
        }

        return new GoogleAdsenseApiException(response.StatusCode, errorCode, message);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private sealed class GoogleAdsenseApiException : Exception
    {
        public GoogleAdsenseApiException(HttpStatusCode statusCode, string? errorCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public HttpStatusCode StatusCode { get; }
        public string? ErrorCode { get; }
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = "";

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }
    }

    private sealed class GoogleAdsenseAccountsResponse
    {
        [JsonPropertyName("accounts")]
        public List<GoogleAdsenseAccountDto> Accounts { get; init; } = [];
    }

    private sealed class GoogleAdsenseAccountDto
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("displayName")]
        public string DisplayName { get; init; } = "";
    }

    private sealed class GoogleAdsenseReportResponse
    {
        [JsonPropertyName("headers")]
        public List<GoogleAdsenseReportHeader> Headers { get; init; } = [];

        [JsonPropertyName("rows")]
        public List<GoogleAdsenseReportRow> Rows { get; init; } = [];

        [JsonPropertyName("totals")]
        public GoogleAdsenseReportRow? Totals { get; init; }
    }

    private sealed class GoogleAdsenseReportHeader
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; init; }
    }

    private sealed class GoogleAdsenseReportRow
    {
        [JsonPropertyName("cells")]
        public List<GoogleAdsenseReportCell> Cells { get; init; } = [];
    }

    private sealed class GoogleAdsenseReportCell
    {
        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }
}
