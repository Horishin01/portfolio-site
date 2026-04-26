namespace PortfolioSite.Models.Adsense;

public sealed class GoogleAdsenseDashboardSnapshot
{
    public bool IsConfigured { get; init; }
    public GoogleAdsenseConnectionSnapshot? Connection { get; init; }
    public GoogleAdsenseSummarySnapshot? Summary { get; init; }
    public IReadOnlyList<GoogleAdsenseDailyMetric> DailyMetrics { get; init; } = [];
    public IReadOnlyList<GoogleAdsenseSiteMetric> TopSites { get; init; } = [];
    public string? ErrorMessage { get; init; }

    public bool IsConnected => Connection is not null;
}

public sealed class GoogleAdsenseConnectionSnapshot
{
    public string AccountName { get; init; } = "";
    public string AccountDisplayName { get; init; } = "";
    public DateTime ConnectedAtUtc { get; init; }
}

public sealed class GoogleAdsenseSummarySnapshot
{
    public long PageViews { get; init; }
    public long Impressions { get; init; }
    public long Clicks { get; init; }
    public decimal EstimatedEarnings { get; init; }
    public decimal PageViewsRpm { get; init; }
    public string? CurrencyCode { get; init; }
}

public sealed class GoogleAdsenseDailyMetric
{
    public string DateLabel { get; init; } = "";
    public long PageViews { get; init; }
    public long Clicks { get; init; }
    public decimal EstimatedEarnings { get; init; }
}

public sealed class GoogleAdsenseSiteMetric
{
    public string Domain { get; init; } = "";
    public long PageViews { get; init; }
    public long Clicks { get; init; }
    public decimal EstimatedEarnings { get; init; }
}
