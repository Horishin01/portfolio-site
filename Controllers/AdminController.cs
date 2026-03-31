using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortfolioSite.Models.Content;
using PortfolioSite.Models.Identity;
using PortfolioSite.Services;
using PortfolioSite.ViewModels;

namespace PortfolioSite.Controllers;

[Authorize(Roles = AdminRoles.Administrator)]
[Route("admin")]
public sealed class AdminController : Controller
{
    private const string FaviconDirectory = "uploads/favicons";
    private const string HeroImageDirectory = "uploads/hero-images";
    private static readonly HashSet<string> AllowedFaviconExtensions =
    [
        ".ico",
        ".png",
        ".svg",
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

    private readonly PortfolioContentService _contentService;
    private readonly IWebHostEnvironment _environment;
    private readonly SignInManager<AdminUser> _signInManager;

    public AdminController(
        PortfolioContentService contentService,
        IWebHostEnvironment environment,
        SignInManager<AdminUser> signInManager
    )
    {
        _contentService = contentService;
        _environment = environment;
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
    [ValidateAntiForgeryToken]
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(PortfolioEditorViewModel model, CancellationToken cancellationToken)
    {
        const string faviconSrcKey = "Document.FaviconSrc";
        const string heroImageSrcKey = "Document.Profile.HeroImageSrc";
        const string heroImageAltKey = "Document.Profile.HeroImageAlt";

        model.Normalize();

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

        ModelState.Remove(faviconSrcKey);
        ModelState.Remove(heroImageSrcKey);
        ModelState.Remove(heroImageAltKey);
        SetLayoutViewData(model.Document);

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
            "対応形式は ICO / PNG / SVG / WEBP / JPG / GIF です。",
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

    private void SetLayoutViewData(PortfolioDocument document)
    {
        ViewData["FaviconHref"] = document.FaviconSrc;
    }
}
