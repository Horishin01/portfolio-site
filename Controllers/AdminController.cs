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
        model.Normalize();

        if (!ModelState.IsValid)
        {
            return View(model);
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
            return View(model);
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
        return RedirectToAction(nameof(Login));
    }

    private async Task<PortfolioEditorViewModel> BuildEditorViewModelAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        return PortfolioEditorViewModel.FromSnapshot(snapshot);
    }
}
