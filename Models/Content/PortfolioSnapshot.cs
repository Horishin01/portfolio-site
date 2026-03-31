namespace PortfolioSite.Models.Content;

public sealed class PortfolioSnapshot
{
    public PortfolioDocument Document { get; init; } = new();
    public DateTime UpdatedAtUtc { get; init; } = DateTime.UtcNow;
}
