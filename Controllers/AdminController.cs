using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using PortfolioSite.Models.Content;
using PortfolioSite.Models.Identity;
using PortfolioSite.Services;
using PortfolioSite.Security;
using PortfolioSite.ViewModels;

namespace PortfolioSite.Controllers;

[Authorize(Roles = AdminRoles.Administrator)]
[Route("admin")]
public sealed class AdminController : Controller
{
    private const string AdsenseOAuthStateCookieName = "portfolio_adsense_oauth_state";
    private const string FaviconDirectory = "uploads/favicons";
    private const string HeroImageDirectory = "uploads/hero-images";
    private const string PersonalItemImageDirectory = "uploads/personal-items";
    private static readonly HashSet<string> AllowedFaviconExtensions =
    [
        ".ico",
        ".png",
        ".webp",
        ".jpg",
        ".jpeg",
        ".gif"
    ];
    private static readonly HashSet<string> AllowedHeroImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    ];
    private const long MaxFaviconBytes = 1 * 1024 * 1024;
    private const long MaxHeroImageBytes = 5 * 1024 * 1024;
    private const long MaxPersonalItemImageBytes = 5 * 1024 * 1024;

    private readonly GoogleAdsenseService _adsenseService;
    private readonly PortfolioContentService _contentService;
    private readonly IWebHostEnvironment _environment;
    private readonly SignInManager<AdminUser> _signInManager;
    private readonly UserManager<AdminUser> _userManager;

    public AdminController(
        GoogleAdsenseService adsenseService,
        PortfolioContentService contentService,
        IWebHostEnvironment environment,
        SignInManager<AdminUser> signInManager,
        UserManager<AdminUser> userManager
    )
    {
        _adsenseService = adsenseService;
        _contentService = contentService;
        _environment = environment;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AdminRoles.Administrator))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.LoginId,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "試行回数が上限に達しました。15 分後に再試行してください。");
            model.Password = "";
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "ID またはパスワードが違います。");
            model.Password = "";
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        SetLayoutViewData(snapshot.Document);
        return View(new AdminDashboardViewModel
        {
            LoginId = User.Identity?.Name ?? "",
            SiteTitle = snapshot.Document.SiteTitle,
            UpdatedAtUtc = snapshot.UpdatedAtUtc
        });
    }

    [Authorize]
    [HttpGet("edit")]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var model = await BuildEditorViewModelAsync(cancellationToken);
        SetLayoutViewData(model.Document);
        return View(model);
    }

    [Authorize]
    [HttpGet("account")]
    public async Task<IActionResult> Account(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        SetLayoutViewData(snapshot.Document);

        var model = new AdminAccountViewModel
        {
            CurrentLoginId = User.Identity?.Name ?? "",
            LoginIdForm = new AdminChangeLoginIdViewModel
            {
                NewLoginId = User.Identity?.Name ?? ""
            }
        };
        await PopulateTwoFactorPreparationAsync(model);

        return View(model);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("account/two-factor/prepare")]
    public async Task<IActionResult> PrepareTwoFactorAuthenticator()
    {
        var user = await GetCurrentAdminUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var resetResult = await _userManager.ResetAuthenticatorKeyAsync(user);
        if (!resetResult.Succeeded)
        {
            AddIdentityErrors(resetResult);
            return await RenderAccountViewAsync();
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["StatusMessage"] = "認証アプリ登録用キーを発行しました。まだログイン時の二段階認証は有効化していません。";
        TempData["StatusTone"] = "success";
        return RedirectToAction(nameof(Account));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("account/login-id")]
    public async Task<IActionResult> ChangeLoginId(
        [Bind(Prefix = "LoginIdForm")] AdminChangeLoginIdViewModel model,
        CancellationToken cancellationToken
    )
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        SetLayoutViewData(snapshot.Document);

        if (!ModelState.IsValid)
        {
            return await RenderAccountViewAsync(loginIdForm: model);
        }

        var user = await GetCurrentAdminUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
        {
            ModelState.AddModelError("LoginIdForm.CurrentPassword", "現在のパスワードが違います。");
            model.CurrentPassword = "";
            return await RenderAccountViewAsync(loginIdForm: model);
        }

        var newLoginId = model.NewLoginId.Trim();
        var existingUser = await _userManager.FindByNameAsync(newLoginId);
        if (existingUser is not null && !string.Equals(existingUser.Id, user.Id, StringComparison.Ordinal))
        {
            ModelState.AddModelError("LoginIdForm.NewLoginId", "このIDはすでに使用されています。");
            model.CurrentPassword = "";
            return await RenderAccountViewAsync(loginIdForm: model);
        }

        var setNameResult = await _userManager.SetUserNameAsync(user, newLoginId);
        if (!setNameResult.Succeeded)
        {
            AddIdentityErrors(setNameResult);
            model.CurrentPassword = "";
            return await RenderAccountViewAsync(loginIdForm: model);
        }

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            AddIdentityErrors(stampResult);
            model.CurrentPassword = "";
            return await RenderAccountViewAsync(loginIdForm: model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "ログインIDを変更しました。";
        TempData["StatusTone"] = "success";
        TempData["AdminSessionEvent"] = "account-updated";
        return RedirectToAction(nameof(Account));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("account/password")]
    public async Task<IActionResult> ChangePassword(
        [Bind(Prefix = "PasswordForm")] AdminChangePasswordViewModel model,
        CancellationToken cancellationToken
    )
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        SetLayoutViewData(snapshot.Document);

        if (!ModelState.IsValid)
        {
            return await RenderAccountViewAsync(passwordForm: model);
        }

        var user = await GetCurrentAdminUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            model.CurrentPassword = "";
            model.NewPassword = "";
            model.ConfirmPassword = "";
            return await RenderAccountViewAsync(passwordForm: model);
        }

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            AddIdentityErrors(stampResult);
            model.CurrentPassword = "";
            model.NewPassword = "";
            model.ConfirmPassword = "";
            return await RenderAccountViewAsync(passwordForm: model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "パスワードを変更しました。";
        TempData["StatusTone"] = "success";
        TempData["AdminSessionEvent"] = "account-updated";
        return RedirectToAction(nameof(Account));
    }

    [Authorize]
    [HttpGet("adsense")]
    public async Task<IActionResult> Adsense(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        var dashboard = await _adsenseService.GetDashboardAsync(cancellationToken);
        var model = AdminAdsenseViewModel.FromSnapshot(dashboard, snapshot.Document.Adsense, BuildAdsenseRedirectUri());
        SetLayoutViewData(snapshot.Document);
        return View(model);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("adsense/code")]
    public async Task<IActionResult> AdsenseCode(AdminAdsenseViewModel model, CancellationToken cancellationToken)
    {
        var adsense = model.ToAdsenseContent();
        if (!string.IsNullOrWhiteSpace(adsense.PublisherId)
            && !GoogleAdsensePublisherId.IsValid(adsense.PublisherId))
        {
            ModelState.AddModelError(
                nameof(AdminAdsenseViewModel.AdsensePublisherId),
                "Publisher ID は ca-pub- から始まる 16 桁の数字で入力してください。"
            );
        }

        if (adsense.IsEnabled && !GoogleAdsensePublisherId.IsValid(adsense.PublisherId))
        {
            ModelState.AddModelError(
                nameof(AdminAdsenseViewModel.AdsensePublisherId),
                "AdSense を有効にする場合は、有効な Publisher ID を入力してください。"
            );
        }

        if (!ModelState.IsValid)
        {
            var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
            var dashboard = await _adsenseService.GetDashboardAsync(cancellationToken);
            var viewModel = AdminAdsenseViewModel.FromSnapshot(dashboard, adsense, BuildAdsenseRedirectUri());
            SetLayoutViewData(snapshot.Document);
            return View(nameof(Adsense), viewModel);
        }

        var updatedSnapshot = await _contentService.SaveAdsenseSettingsAsync(adsense, cancellationToken);
        SetLayoutViewData(updatedSnapshot.Document);
        TempData["StatusMessage"] = adsense.IsEnabled
            ? "AdSense Publisher ID を保存し、公開ページへの出力を有効にしました。"
            : "AdSense Publisher ID を保存しました。公開ページへの出力は無効です。";
        TempData["StatusTone"] = "success";
        return RedirectToAction(nameof(Adsense));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("adsense/connect")]
    public IActionResult AdsenseConnect()
    {
        if (!_adsenseService.IsConfigured)
        {
            TempData["StatusMessage"] = "Google AdSense OAuth クライアントが未設定です。env または user-secrets を設定してください。";
            TempData["StatusTone"] = "warning";
            return RedirectToAction(nameof(Adsense));
        }

        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        Response.Cookies.Append(
            AdsenseOAuthStateCookieName,
            state,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(10),
                Path = Url.Action(nameof(AdsenseCallback)) ?? "/admin/adsense/callback",
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            }
        );

        return Redirect(_adsenseService.BuildAuthorizationUrl(BuildAdsenseRedirectUri(), state));
    }

    [Authorize]
    [HttpGet("adsense/callback")]
    public async Task<IActionResult> AdsenseCallback(
        string? code,
        string? state,
        string? error,
        CancellationToken cancellationToken
    )
    {
        var expectedState = Request.Cookies[AdsenseOAuthStateCookieName];
        Response.Cookies.Delete(
            AdsenseOAuthStateCookieName,
            new CookieOptions
            {
                Path = Url.Action(nameof(AdsenseCallback)) ?? "/admin/adsense/callback",
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            }
        );

        if (!AdsenseOAuthStateMatches(expectedState, state))
        {
            TempData["StatusMessage"] = "Google OAuth の state 検証に失敗しました。再度接続してください。";
            TempData["StatusTone"] = "error";
            return RedirectToAction(nameof(Adsense));
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            TempData["StatusMessage"] = string.Equals(error, "access_denied", StringComparison.OrdinalIgnoreCase)
                ? "Google AdSense へのアクセスが承認されませんでした。"
                : "Google OAuth でエラーが返されました。設定を確認して再度接続してください。";
            TempData["StatusTone"] = "warning";
            return RedirectToAction(nameof(Adsense));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["StatusMessage"] = "Google OAuth の認証コードを受け取れませんでした。";
            TempData["StatusTone"] = "error";
            return RedirectToAction(nameof(Adsense));
        }

        try
        {
            await _adsenseService.ConnectAsync(code, BuildAdsenseRedirectUri(), cancellationToken);
            TempData["StatusMessage"] = "Google AdSense との接続を保存しました。";
            TempData["StatusTone"] = "success";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = ex.Message;
            TempData["StatusTone"] = "error";
        }

        return RedirectToAction(nameof(Adsense));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("adsense/disconnect")]
    public async Task<IActionResult> AdsenseDisconnect(CancellationToken cancellationToken)
    {
        await _adsenseService.DisconnectAsync(cancellationToken);
        TempData["StatusMessage"] = "Google AdSense 接続を解除しました。";
        TempData["StatusTone"] = "warning";
        return RedirectToAction(nameof(Adsense));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(
        PortfolioEditorViewModel model,
        string? editorAction,
        int? editorScrollY,
        CancellationToken cancellationToken
    )
    {
        const string faviconSrcKey = "Document.FaviconSrc";
        const string heroImageSrcKey = "Document.Profile.HeroImageSrc";
        const string heroImageAltKey = "Document.Profile.HeroImageAlt";

        model.Normalize();

        if (!string.IsNullOrWhiteSpace(editorAction))
        {
            var (statusMessage, statusTone) = ApplyEditorAction(model, editorAction);
            model.ApplyStructuredEntriesToDocument();
            ModelState.Clear();
            return RenderEditView(model, statusMessage, statusTone, editorScrollY);
        }

        model.ApplyStructuredEntriesToDocument();

        try
        {
            var uploadedFaviconPath = await SaveFaviconAsync(
                model.FaviconFile,
                model.Document.FaviconSrc,
                cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(uploadedFaviconPath))
            {
                model.Document.FaviconSrc = uploadedFaviconPath;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(model.FaviconFile), ex.Message);
        }

        try
        {
            var uploadedHeroImagePath = await SaveHeroImageAsync(
                model.HeroImageFile,
                model.Document.Profile.HeroImageSrc,
                cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(uploadedHeroImagePath))
            {
                model.Document.Profile.HeroImageSrc = uploadedHeroImagePath;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(model.HeroImageFile), ex.Message);
        }

        for (var itemIndex = 0; itemIndex < model.Document.PersonalSection.Items.Count; itemIndex++)
        {
            var personalItem = model.Document.PersonalSection.Items[itemIndex];
            var fileFieldName = GetPersonalItemImageFieldName(itemIndex);

            try
            {
                var uploadedPersonalImagePath = await SavePersonalItemImageAsync(
                    Request.Form.Files.GetFile(fileFieldName),
                    personalItem.ImageSrc,
                    cancellationToken
                );

                if (!string.IsNullOrWhiteSpace(uploadedPersonalImagePath))
                {
                    personalItem.ImageSrc = uploadedPersonalImagePath;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(fileFieldName, ex.Message);
            }

            ModelState.Remove($"Document.PersonalSection.Items[{itemIndex}].ImageSrc");
            ModelState.Remove($"Document.PersonalSection.Items[{itemIndex}].ImageAlt");
        }

        ValidateAndSanitizeDocumentUrls(model.Document);

        ModelState.Remove(faviconSrcKey);
        ModelState.Remove(heroImageSrcKey);
        ModelState.Remove(heroImageAltKey);
        SetLayoutViewData(model.Document);

        if (!ModelState.IsValid)
        {
            return RenderEditView(model);
        }

        try
        {
            await _contentService.SaveAsync(model.Document, cancellationToken);
            TempData["StatusMessage"] = "ポートフォリオを保存しました。";
            TempData["StatusTone"] = "success";

            return RedirectToAction(nameof(Edit));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"保存に失敗しました: {ex.Message}");
            return RenderEditView(model);
        }
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _contentService.ResetToDefaultAsync(cancellationToken);
        TempData["StatusMessage"] = "初期データに戻しました。";
        TempData["StatusTone"] = "warning";
        return RedirectToAction(nameof(Edit));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["AdminSessionEvent"] = "logout";
        return RedirectToAction(nameof(Login));
    }

    private async Task<PortfolioEditorViewModel> BuildEditorViewModelAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        return PortfolioEditorViewModel.FromSnapshot(snapshot);
    }

    private IActionResult RenderEditView(
        PortfolioEditorViewModel model,
        string? statusMessage = null,
        string? statusTone = null,
        int? restoreScrollY = null
    )
    {
        SetLayoutViewData(model.Document);

        if (!string.IsNullOrWhiteSpace(statusMessage))
        {
            ViewData["StatusMessage"] = statusMessage;
        }

        if (!string.IsNullOrWhiteSpace(statusTone))
        {
            ViewData["StatusTone"] = statusTone;
        }

        if (restoreScrollY is >= 0)
        {
            ViewData["RestoreScrollY"] = restoreScrollY.Value;
        }

        return View("Edit", model);
    }

    private AdminAccountViewModel BuildAccountViewModel(
        AdminChangeLoginIdViewModel? loginIdForm = null,
        AdminChangePasswordViewModel? passwordForm = null
    )
    {
        return new AdminAccountViewModel
        {
            CurrentLoginId = User.Identity?.Name ?? "",
            LoginIdForm = loginIdForm ?? new AdminChangeLoginIdViewModel
            {
                NewLoginId = User.Identity?.Name ?? ""
            },
            PasswordForm = passwordForm ?? new AdminChangePasswordViewModel()
        };
    }

    private async Task<IActionResult> RenderAccountViewAsync(
        AdminChangeLoginIdViewModel? loginIdForm = null,
        AdminChangePasswordViewModel? passwordForm = null
    )
    {
        var model = BuildAccountViewModel(loginIdForm, passwordForm);
        await PopulateTwoFactorPreparationAsync(model);
        return View(nameof(Account), model);
    }

    private async Task PopulateTwoFactorPreparationAsync(AdminAccountViewModel model)
    {
        var user = await GetCurrentAdminUserAsync();
        if (user is null)
        {
            return;
        }

        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user) ?? "";
        var formattedKey = TotpAuthenticatorUriBuilder.FormatSharedKey(authenticatorKey);
        model.TwoFactor = new AdminTwoFactorPreparationViewModel
        {
            IsEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            HasAuthenticatorKey = !string.IsNullOrWhiteSpace(authenticatorKey),
            SharedKey = formattedKey,
            AuthenticatorUri = string.IsNullOrWhiteSpace(authenticatorKey)
                ? ""
                : TotpAuthenticatorUriBuilder.BuildUri("Portfolio Admin", user.UserName ?? model.CurrentLoginId, authenticatorKey)
        };
    }

    private async Task<AdminUser?> GetCurrentAdminUserAsync()
    {
        return await _userManager.GetUserAsync(User);
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, MapIdentityError(error));
        }
    }

    private static string MapIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            nameof(IdentityErrorDescriber.PasswordTooShort) => "パスワードが短すぎます。",
            nameof(IdentityErrorDescriber.PasswordRequiresDigit) => "パスワードには数字を含めてください。",
            nameof(IdentityErrorDescriber.PasswordRequiresLower) => "パスワードには小文字を含めてください。",
            nameof(IdentityErrorDescriber.PasswordRequiresUpper) => "パスワードには大文字を含めてください。",
            nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => "パスワードには記号を含めてください。",
            nameof(IdentityErrorDescriber.InvalidUserName) => "IDに使用できない文字が含まれています。",
            nameof(IdentityErrorDescriber.DuplicateUserName) => "このIDはすでに使用されています。",
            nameof(IdentityErrorDescriber.PasswordMismatch) => "現在のパスワードが違います。",
            _ => string.IsNullOrWhiteSpace(error.Description)
                ? "アカウント情報の更新に失敗しました。"
                : error.Description
        };
    }

    private static (string Message, string Tone) ApplyEditorAction(
        PortfolioEditorViewModel model,
        string editorAction
    )
    {
        var parts = editorAction.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return ("操作を判別できませんでした。", "warning");
        }

        var verb = parts[0];
        var target = parts[1];

        if (verb.Equals("add", StringComparison.OrdinalIgnoreCase))
        {
            return target switch
            {
                "profile-highlight" => AddProfileHighlight(model),
                "career-item" => AddCareerItem(model),
                "certification" => AddCertification(model),
                "profile-tag" => AddProfileTag(model),
                "skill-category" => AddSkillCategory(model),
                "skill-item" => AddSkillItem(model, parts),
                "work-item" => AddWorkItem(model),
                "personal-item" => AddPersonalItem(model),
                "contact-link" => AddContactLink(model),
                _ => ("追加対象を判別できませんでした。", "warning")
            };
        }

        if (verb.Equals("remove", StringComparison.OrdinalIgnoreCase))
        {
            return target switch
            {
                "profile-highlight" => RemoveProfileHighlight(model, parts),
                "career-item" => RemoveCareerItem(model, parts),
                "certification" => RemoveCertification(model, parts),
                "profile-tag" => RemoveProfileTag(model, parts),
                "skill-category" => RemoveSkillCategory(model, parts),
                "skill-item" => RemoveSkillItem(model, parts),
                "work-item" => RemoveWorkItem(model, parts),
                "personal-item" => RemovePersonalItem(model, parts),
                "contact-link" => RemoveContactLink(model, parts),
                _ => ("削除対象を判別できませんでした。", "warning")
            };
        }

        return ("操作を判別できませんでした。", "warning");
    }

    private static (string Message, string Tone) AddProfileHighlight(PortfolioEditorViewModel model)
    {
        model.Document.Profile.Highlights.Add(new LabeledValueItem());
        return ("プロフィール指標を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveProfileHighlight(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.Profile.Highlights.Count, out var index))
        {
            return ("削除するプロフィール指標が見つかりませんでした。", "warning");
        }

        model.Document.Profile.Highlights.RemoveAt(index);
        return ("プロフィール指標を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddCareerItem(PortfolioEditorViewModel model)
    {
        model.Document.CareerSection.Items.Add(new CareerItem());
        return ("在籍歴項目を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveCareerItem(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.CareerSection.Items.Count, out var index))
        {
            return ("削除する在籍歴項目が見つかりませんでした。", "warning");
        }

        model.Document.CareerSection.Items.RemoveAt(index);
        return ("在籍歴項目を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddCertification(PortfolioEditorViewModel model)
    {
        model.CertificationEntries.Add("");
        return ("資格入力欄を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveCertification(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.CertificationEntries.Count, out var index))
        {
            return ("削除する資格が見つかりませんでした。", "warning");
        }

        model.CertificationEntries.RemoveAt(index);
        return ("資格を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddProfileTag(PortfolioEditorViewModel model)
    {
        model.ProfileTagEntries.Add("");
        return ("タグ入力欄を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveProfileTag(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.ProfileTagEntries.Count, out var index))
        {
            return ("削除するタグが見つかりませんでした。", "warning");
        }

        model.ProfileTagEntries.RemoveAt(index);
        return ("タグを削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddSkillCategory(PortfolioEditorViewModel model)
    {
        model.Document.SkillsSection.Categories.Add(new SkillCategory
        {
            Items = [new SkillItem()]
        });

        return ("スキルカテゴリを追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveSkillCategory(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.SkillsSection.Categories.Count, out var index))
        {
            return ("削除するスキルカテゴリが見つかりませんでした。", "warning");
        }

        model.Document.SkillsSection.Categories.RemoveAt(index);
        return ("スキルカテゴリを削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddSkillItem(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.SkillsSection.Categories.Count, out var categoryIndex))
        {
            return ("スキル項目を追加するカテゴリが見つかりませんでした。", "warning");
        }

        model.Document.SkillsSection.Categories[categoryIndex].Items.Add(new SkillItem());
        return ("スキル項目を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveSkillItem(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.SkillsSection.Categories.Count, out var categoryIndex))
        {
            return ("削除するスキルカテゴリが見つかりませんでした。", "warning");
        }

        var items = model.Document.SkillsSection.Categories[categoryIndex].Items;
        if (!TryGetIndex(parts, 3, items.Count, out var itemIndex))
        {
            return ("削除するスキル項目が見つかりませんでした。", "warning");
        }

        items.RemoveAt(itemIndex);
        return ("スキル項目を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddWorkItem(PortfolioEditorViewModel model)
    {
        model.Document.WorksSection.Items.Add(new WorkItem());
        return ("実績項目を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveWorkItem(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.WorksSection.Items.Count, out var index))
        {
            return ("削除する実績項目が見つかりませんでした。", "warning");
        }

        model.Document.WorksSection.Items.RemoveAt(index);
        return ("実績項目を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddPersonalItem(PortfolioEditorViewModel model)
    {
        model.Document.PersonalSection.Items.Add(new PersonalItem());
        return ("個人活動項目を追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemovePersonalItem(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.PersonalSection.Items.Count, out var index))
        {
            return ("削除する個人活動項目が見つかりませんでした。", "warning");
        }

        model.Document.PersonalSection.Items.RemoveAt(index);
        return ("個人活動項目を削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static (string Message, string Tone) AddContactLink(PortfolioEditorViewModel model)
    {
        model.Document.Contact.Links.Add(new LinkItem());
        return ("連絡先リンクを追加しました。保存すると公開ページへ反映されます。", "success");
    }

    private static (string Message, string Tone) RemoveContactLink(PortfolioEditorViewModel model, string[] parts)
    {
        if (!TryGetIndex(parts, 2, model.Document.Contact.Links.Count, out var index))
        {
            return ("削除する連絡先リンクが見つかりませんでした。", "warning");
        }

        model.Document.Contact.Links.RemoveAt(index);
        return ("連絡先リンクを削除しました。保存すると公開ページへ反映されます。", "warning");
    }

    private static bool TryGetIndex(string[] parts, int position, int upperBoundExclusive, out int index)
    {
        index = -1;

        if (parts.Length <= position || !int.TryParse(parts[position], out index))
        {
            return false;
        }

        return index >= 0 && index < upperBoundExclusive;
    }

    private async Task<string?> SaveFaviconAsync(
        IFormFile? faviconFile,
        string currentFaviconSrc,
        CancellationToken cancellationToken
    )
    {
        return await SaveManagedImageAsync(
            faviconFile,
            currentFaviconSrc,
            FaviconDirectory,
            AllowedFaviconExtensions,
            MaxFaviconBytes,
            "アイコンサイズは 1 MB 以下にしてください。",
            "対応形式は ICO / PNG / WEBP / JPG / GIF です。",
            cancellationToken
        );
    }

    private async Task<string?> SaveHeroImageAsync(
        IFormFile? heroImageFile,
        string currentHeroImageSrc,
        CancellationToken cancellationToken
    )
    {
        return await SaveManagedImageAsync(
            heroImageFile,
            currentHeroImageSrc,
            HeroImageDirectory,
            AllowedHeroImageExtensions,
            MaxHeroImageBytes,
            "画像サイズは 5 MB 以下にしてください。",
            "対応形式は JPG / PNG / WEBP / GIF です。",
            cancellationToken
        );
    }

    private async Task<string?> SavePersonalItemImageAsync(
        IFormFile? personalImageFile,
        string currentPersonalImageSrc,
        CancellationToken cancellationToken
    )
    {
        return await SaveManagedImageAsync(
            personalImageFile,
            currentPersonalImageSrc,
            PersonalItemImageDirectory,
            AllowedHeroImageExtensions,
            MaxPersonalItemImageBytes,
            "写真サイズは 5 MB 以下にしてください。",
            "対応形式は JPG / PNG / WEBP / GIF です。",
            cancellationToken
        );
    }

    private async Task<string?> SaveManagedImageAsync(
        IFormFile? imageFile,
        string currentImageSrc,
        string uploadDirectory,
        IReadOnlySet<string> allowedExtensions,
        long maxBytes,
        string maxBytesMessage,
        string formatMessage,
        CancellationToken cancellationToken
    )
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            return null;
        }

        if (imageFile.Length > maxBytes)
        {
            throw new InvalidOperationException(maxBytesMessage);
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(formatMessage);
        }

        if (!await HasAllowedImageSignatureAsync(imageFile, extension, cancellationToken))
        {
            throw new InvalidOperationException(formatMessage);
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, uploadDirectory);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await imageFile.CopyToAsync(fileStream, cancellationToken);
        }

        DeleteManagedImage(currentImageSrc, uploadDirectory);

        return $"/{uploadDirectory.Replace("\\", "/", StringComparison.Ordinal)}/{fileName}";
    }

    private static async Task<bool> HasAllowedImageSignatureAsync(
        IFormFile imageFile,
        string extension,
        CancellationToken cancellationToken
    )
    {
        var buffer = new byte[16];
        await using var stream = imageFile.OpenReadStream();
        var length = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        return extension switch
        {
            ".jpg" or ".jpeg" => HasPrefix(buffer, length, [0xFF, 0xD8, 0xFF]),
            ".png" => HasPrefix(buffer, length, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            ".gif" => HasAsciiPrefix(buffer, length, "GIF87a") || HasAsciiPrefix(buffer, length, "GIF89a"),
            ".webp" => HasWebpSignature(buffer, length),
            ".ico" => HasPrefix(buffer, length, [0x00, 0x00, 0x01, 0x00]),
            _ => false
        };
    }

    private static bool HasPrefix(byte[] value, int length, ReadOnlySpan<byte> prefix)
    {
        return length >= prefix.Length && value.AsSpan(0, prefix.Length).SequenceEqual(prefix);
    }

    private static bool HasAsciiPrefix(byte[] value, int length, string prefix)
    {
        return length >= prefix.Length
            && value.AsSpan(0, prefix.Length).SequenceEqual(Encoding.ASCII.GetBytes(prefix));
    }

    private static bool HasWebpSignature(byte[] value, int length)
    {
        return length >= 12
            && HasAsciiPrefix(value, length, "RIFF")
            && value.AsSpan(8, 4).SequenceEqual(Encoding.ASCII.GetBytes("WEBP"));
    }

    private void DeleteManagedImage(string? imagePath, string uploadDirectory)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        var normalizedPrefix = $"/{uploadDirectory.Replace("\\", "/", StringComparison.Ordinal)}/";
        if (!imagePath.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(imagePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var fullPath = Path.Combine(_environment.WebRootPath, uploadDirectory, fileName);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private static string GetPersonalItemImageFieldName(int index)
    {
        return $"PersonalItemImageFile_{index}";
    }

    private void ValidateAndSanitizeDocumentUrls(PortfolioDocument document)
    {
        SanitizeDocumentField(
            "Document.FaviconSrc",
            document.FaviconSrc,
            sanitized => document.FaviconSrc = sanitized,
            PublicUrlSanitizer.SanitizeFaviconUrl,
            "ファビコンURLは / で始まるサイト内パス、または https の ICO / PNG / WEBP / JPG / GIF を指定してください。"
        );

        SanitizeDocumentField(
            "Document.Profile.HeroImageSrc",
            document.Profile.HeroImageSrc,
            sanitized => document.Profile.HeroImageSrc = sanitized,
            PublicUrlSanitizer.SanitizeImageUrl,
            "ヒーロー画像URLは / で始まるサイト内パス、または https の URL を指定してください。"
        );

        for (var itemIndex = 0; itemIndex < document.PersonalSection.Items.Count; itemIndex++)
        {
            var personalItem = document.PersonalSection.Items[itemIndex];
            SanitizeDocumentField(
                $"Document.PersonalSection.Items[{itemIndex}].ImageSrc",
                personalItem.ImageSrc,
                sanitized => personalItem.ImageSrc = sanitized,
                PublicUrlSanitizer.SanitizeImageUrl,
                "個人活動画像URLは / で始まるサイト内パス、または https の URL を指定してください。"
            );
        }

        for (var linkIndex = 0; linkIndex < document.Contact.Links.Count; linkIndex++)
        {
            var link = document.Contact.Links[linkIndex];
            SanitizeDocumentField(
                $"Document.Contact.Links[{linkIndex}].Href",
                link.Href,
                sanitized => link.Href = sanitized,
                PublicUrlSanitizer.SanitizeLinkUrl,
                "リンクURLは / で始まるサイト内パス、または http/https / mailto / tel の URL を指定してください。"
            );
        }
    }

    private void SanitizeDocumentField(
        string modelKey,
        string? currentValue,
        Action<string> assign,
        Func<string?, string?> sanitizer,
        string errorMessage
    )
    {
        var sanitized = sanitizer(currentValue);
        if (sanitized is null)
        {
            ModelState.AddModelError(modelKey, errorMessage);
            return;
        }

        assign(sanitized);
    }

    private string BuildAdsenseRedirectUri()
    {
        return Url.Action(nameof(AdsenseCallback), "Admin", values: null, protocol: Request.Scheme)
            ?? throw new InvalidOperationException("Google AdSense callback URL を生成できませんでした。");
    }

    private static bool AdsenseOAuthStateMatches(string? expectedState, string? providedState)
    {
        if (string.IsNullOrWhiteSpace(expectedState) || string.IsNullOrWhiteSpace(providedState))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedState);
        var providedBytes = Encoding.UTF8.GetBytes(providedState);
        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private void SetLayoutViewData(PortfolioDocument document)
    {
        ViewData["FaviconHref"] = document.FaviconSrc;
    }
}
