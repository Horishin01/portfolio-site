namespace PortfolioSite.Data;

public sealed class PortfolioContentRecord
{
    public int Id { get; set; } = 1;
    public string JsonContent { get; set; } = "{}";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
