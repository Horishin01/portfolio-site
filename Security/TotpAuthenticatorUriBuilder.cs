using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace PortfolioSite.Security;

public static class TotpAuthenticatorUriBuilder
{
    public static string BuildUri(string issuer, string accountName, string sharedKey)
    {
        var normalizedIssuer = NormalizeLabel(issuer, fallback: "Portfolio Admin");
        var normalizedAccountName = NormalizeLabel(accountName, fallback: "Admin");
        var normalizedKey = NormalizeSharedKey(sharedKey);
        var label = $"{normalizedIssuer}:{normalizedAccountName}";

        var parameters = new Dictionary<string, string?>
        {
            ["secret"] = normalizedKey,
            ["issuer"] = normalizedIssuer,
            ["algorithm"] = "SHA1",
            ["digits"] = "6",
            ["period"] = "30"
        };

        return QueryHelpers.AddQueryString($"otpauth://totp/{Uri.EscapeDataString(label)}", parameters);
    }

    public static string FormatSharedKey(string sharedKey)
    {
        var normalized = NormalizeSharedKey(sharedKey);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "";
        }

        var builder = new StringBuilder(normalized.Length + normalized.Length / 4);
        for (var index = 0; index < normalized.Length; index++)
        {
            if (index > 0 && index % 4 == 0)
            {
                builder.Append(' ');
            }

            builder.Append(normalized[index]);
        }

        return builder.ToString();
    }

    private static string NormalizeSharedKey(string sharedKey)
    {
        return (sharedKey ?? string.Empty)
            .Replace(" ", "", StringComparison.Ordinal)
            .ToUpper(CultureInfo.InvariantCulture);
    }

    private static string NormalizeLabel(string value, string fallback)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? fallback
            : normalized.Replace(':', ' ');
    }
}
