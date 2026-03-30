using System.ComponentModel.DataAnnotations;

namespace PortfolioSite.ViewModels;

public sealed class PortfolioEditorViewModel
{
    [Display(Name = "Portfolio JSON")]
    [Required(ErrorMessage = "JSON を入力してください。")]
    public string JsonContent { get; set; } = "{}";

    public string SiteTitle { get; init; } = "";
    public string MetaDescription { get; init; } = "";
    public DateTime UpdatedAtUtc { get; init; }
}
