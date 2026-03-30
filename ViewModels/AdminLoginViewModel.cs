using System.ComponentModel.DataAnnotations;

namespace PortfolioSite.ViewModels;

public sealed class AdminLoginViewModel
{
    [Display(Name = "ID")]
    [Required(ErrorMessage = "ID を入力してください。")]
    public string LoginId { get; set; } = "";

    [Display(Name = "パスワード")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "パスワードを入力してください。")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
