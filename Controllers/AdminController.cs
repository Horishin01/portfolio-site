using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortfolioSite.Models.Identity;
using PortfolioSite.Services;
using PortfolioSite.ViewModels;

namespace PortfolioSite.Controllers;

[Authorize(Roles = AdminRoles.Administrator)]
[Route("admin")]
public sealed class AdminController : Controller
{
    private readonly PortfolioContentService _contentService;
    private readonly SignInManager<AdminUser> _signInManager;

    public AdminController(
        PortfolioContentService contentService,
        SignInManager<AdminUser> signInManager
    )
    {
        _contentService = contentService;
        _signInManager = signInManager;
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
        return View(await BuildEditorViewModelAsync(cancellationToken));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(PortfolioEditorViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildEditorViewModelAsync(cancellationToken, model.JsonContent));
        }

        try
        {
            await _contentService.SaveJsonAsync(model.JsonContent, cancellationToken);
            TempData["StatusMessage"] = "ポートフォリオを保存しました。";
            TempData["StatusTone"] = "success";

            return RedirectToAction(nameof(Edit));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"JSON の保存に失敗しました: {ex.Message}");
            return View(await BuildEditorViewModelAsync(cancellationToken, model.JsonContent));
        }
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _contentService.ResetToSeedAsync(cancellationToken);
        TempData["StatusMessage"] = "初期データに戻しました。";
        TempData["StatusTone"] = "warning";
        return RedirectToAction(nameof(Edit));
    }

    [Authorize]
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var json = await _contentService.ExportJsonAsync(cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"portfolio-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        return File(bytes, "application/json", fileName);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile? uploadedFile, CancellationToken cancellationToken)
    {
        if (uploadedFile is null || uploadedFile.Length == 0)
        {
            TempData["StatusMessage"] = "読み込む JSON ファイルを選択してください。";
            TempData["StatusTone"] = "error";
            return RedirectToAction(nameof(Edit));
        }

        using var stream = uploadedFile.OpenReadStream();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            await _contentService.SaveJsonAsync(json, cancellationToken);
            TempData["StatusMessage"] = "JSON を読み込みました。";
            TempData["StatusTone"] = "success";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"JSON の読み込みに失敗しました: {ex.Message}";
            TempData["StatusTone"] = "error";
        }

        return RedirectToAction(nameof(Edit));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private async Task<PortfolioEditorViewModel> BuildEditorViewModelAsync(
        CancellationToken cancellationToken,
        string? jsonOverride = null
    )
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        return new PortfolioEditorViewModel
        {
            JsonContent = jsonOverride ?? snapshot.JsonContent,
            SiteTitle = snapshot.Document.SiteTitle,
            MetaDescription = snapshot.Document.MetaDescription,
            UpdatedAtUtc = snapshot.UpdatedAtUtc
        };
    }
}
