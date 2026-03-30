namespace PortfolioSite.ViewModels;

public sealed class AdminDashboardViewModel
{
    public string LoginId { get; init; } = "";
    public string SiteTitle { get; init; } = "";
    public DateTime UpdatedAtUtc { get; init; }
}
