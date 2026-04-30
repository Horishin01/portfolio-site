using System.ComponentModel.DataAnnotations;
using PortfolioSite.Models.Adsense;
using PortfolioSite.Models.Content;
using PortfolioSite.Security;

namespace PortfolioSite.ViewModels;

public sealed class AdminAdsenseViewModel
{
    private const string DefaultLoaderScript =
        "<script async src=\"https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-0000000000000000\" crossorigin=\"anonymous\"></script>";

    public bool IsConfigured { get; init; }
    public bool IsConnected { get; init; }
    public string RedirectUri { get; init; } = "";
    public bool AdsenseIsEnabled { get; set; }

    [Display(Name = "Publisher ID")]
    public string AdsensePublisherId { get; set; } = "";

    [Display(Name = "head 追加コード")]
    public string AdsenseHeadScript { get; set; } = "";

    [Display(Name = "body 末尾追加コード")]
    public string AdsenseBodyScript { get; set; } = "";

    public string? AccountName { get; init; }
    public string? AccountDisplayName { get; init; }
    public DateTime? ConnectedAtUtc { get; init; }
    public DateTime? ConnectedAtJst => ConnectedAtUtc is DateTime value
        ? AdminDateTimeDisplay.ToJst(value)
        : null;
    public long? PageViews30Days { get; init; }
    public long? Impressions30Days { get; init; }
    public long? Clicks30Days { get; init; }
    public decimal? EstimatedEarnings30Days { get; init; }
    public decimal? PageViewsRpm30Days { get; init; }
    public string? CurrencyCode { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<AdminAdsenseDailyMetricViewModel> DailyMetrics { get; init; } = [];
    public IReadOnlyList<AdminAdsenseSiteMetricViewModel> TopSites { get; init; } = [];

    public bool HasMetrics => PageViews30Days.HasValue;
    public bool HasPublishCode => GoogleAdsensePublisherId.IsValid(AdsensePublisherId);

    public static AdminAdsenseViewModel FromSnapshot(
        GoogleAdsenseDashboardSnapshot snapshot,
        AdsenseContent adsense,
        string redirectUri
    )
    {
        return new AdminAdsenseViewModel
        {
            IsConfigured = snapshot.IsConfigured,
            IsConnected = snapshot.IsConnected,
            RedirectUri = redirectUri,
            AdsenseIsEnabled = adsense.IsEnabled,
            AdsensePublisherId = adsense.PublisherId,
            AdsenseHeadScript = adsense.HeadScript,
            AdsenseBodyScript = adsense.BodyScript,
            AccountName = snapshot.Connection?.AccountName,
            AccountDisplayName = snapshot.Connection?.AccountDisplayName,
            ConnectedAtUtc = snapshot.Connection?.ConnectedAtUtc,
            PageViews30Days = snapshot.Summary?.PageViews,
            Impressions30Days = snapshot.Summary?.Impressions,
            Clicks30Days = snapshot.Summary?.Clicks,
            EstimatedEarnings30Days = snapshot.Summary?.EstimatedEarnings,
            PageViewsRpm30Days = snapshot.Summary?.PageViewsRpm,
            CurrencyCode = snapshot.Summary?.CurrencyCode,
            ErrorMessage = snapshot.ErrorMessage,
            DailyMetrics = snapshot.DailyMetrics
                .Select(item => new AdminAdsenseDailyMetricViewModel
                {
                    DateLabel = item.DateLabel,
                    PageViews = item.PageViews,
                    Clicks = item.Clicks,
                    EstimatedEarnings = item.EstimatedEarnings
                })
                .ToList(),
            TopSites = snapshot.TopSites
                .Select(item => new AdminAdsenseSiteMetricViewModel
                {
                    Domain = item.Domain,
                    PageViews = item.PageViews,
                    Clicks = item.Clicks,
                    EstimatedEarnings = item.EstimatedEarnings
                })
                .ToList()
        };
    }

    public AdsenseContent ToAdsenseContent()
    {
        return new AdsenseContent
        {
            IsEnabled = AdsenseIsEnabled,
            PublisherId = AdsensePublisherId?.Trim() ?? "",
            HeadScript = "",
            BodyScript = ""
        };
    }

    public string BuildSuggestedHeadScript()
    {
        if (string.IsNullOrWhiteSpace(AdsensePublisherId))
        {
            return DefaultLoaderScript;
        }

        return DefaultLoaderScript.Replace("ca-pub-0000000000000000", AdsensePublisherId.Trim(), StringComparison.Ordinal);
    }
}

public sealed class AdminAdsenseDailyMetricViewModel
{
    public string DateLabel { get; init; } = "";
    public long PageViews { get; init; }
    public long Clicks { get; init; }
    public decimal EstimatedEarnings { get; init; }
}

public sealed class AdminAdsenseSiteMetricViewModel
{
    public string Domain { get; init; } = "";
    public long PageViews { get; init; }
    public long Clicks { get; init; }
    public decimal EstimatedEarnings { get; init; }
}
