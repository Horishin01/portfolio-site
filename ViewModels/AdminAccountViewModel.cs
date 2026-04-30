using System.ComponentModel.DataAnnotations;

namespace PortfolioSite.ViewModels;

public sealed class AdminAccountViewModel
{
    public string CurrentLoginId { get; init; } = "";
    public AdminChangeLoginIdViewModel LoginIdForm { get; set; } = new();
    public AdminChangePasswordViewModel PasswordForm { get; set; } = new();
    public AdminTwoFactorPreparationViewModel TwoFactor { get; set; } = new();
}

public sealed class AdminChangeLoginIdViewModel
{
    [Display(Name = "新しいID")]
    [Required(ErrorMessage = "新しいIDを入力してください。")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "IDは3文字以上128文字以内で入力してください。")]
    public string NewLoginId { get; set; } = "";

    [Display(Name = "現在のパスワード")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "現在のパスワードを入力してください。")]
    public string CurrentPassword { get; set; } = "";
}

public sealed class AdminChangePasswordViewModel
{
    [Display(Name = "現在のパスワード")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "現在のパスワードを入力してください。")]
    public string CurrentPassword { get; set; } = "";

    [Display(Name = "新しいパスワード")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "新しいパスワードを入力してください。")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "パスワードは12文字以上128文字以内で入力してください。")]
    public string NewPassword { get; set; } = "";

    [Display(Name = "新しいパスワード（確認）")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "確認用パスワードを入力してください。")]
    [Compare(nameof(NewPassword), ErrorMessage = "新しいパスワードと確認用パスワードが一致しません。")]
    public string ConfirmPassword { get; set; } = "";
}

public sealed class AdminTwoFactorPreparationViewModel
{
    public bool IsEnabled { get; init; }
    public bool HasAuthenticatorKey { get; init; }
    public string SharedKey { get; init; } = "";
    public string AuthenticatorUri { get; init; } = "";
    public string[] SupportedApps { get; init; } =
    [
        "Google Authenticator",
        "Microsoft Authenticator",
        "1Password",
        "Authy"
    ];
}
