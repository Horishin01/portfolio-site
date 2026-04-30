using System.Text.RegularExpressions;

namespace PortfolioSite.Security;

public static partial class GoogleAdsensePublisherId
{
    public static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && PublisherIdPattern().IsMatch(value.Trim());
    }

    public static string Normalize(string? value)
    {
        var normalized = value?.Trim() ?? "";
        return IsValid(normalized) ? normalized : "";
    }

    [GeneratedRegex(@"^ca-pub-\d{16}$", RegexOptions.CultureInvariant)]
    private static partial Regex PublisherIdPattern();
}
