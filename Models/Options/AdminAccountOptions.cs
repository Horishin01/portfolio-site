namespace PortfolioSite.Models.Options;

public sealed class AdminAccountOptions
{
    public const string SectionName = "AdminAccount";

    public string LoginId { get; set; } = "admin";
    public string PasswordHash { get; set; } = "";
}
