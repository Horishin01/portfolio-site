namespace PortfolioSite.Data;

public sealed class AdminAdsenseConnectionRecord
{
    public int Id { get; set; } = 1;
    public string AccountName { get; set; } = "";
    public string AccountDisplayName { get; set; } = "";
    public string RefreshTokenCiphertext { get; set; } = "";
    public DateTime ConnectedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
