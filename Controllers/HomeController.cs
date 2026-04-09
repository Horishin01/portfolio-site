using Microsoft.AspNetCore.Mvc;
using PortfolioSite.Models.Content;
using PortfolioSite.Services;
using PortfolioSite.ViewModels;

namespace PortfolioSite.Controllers;

public sealed class HomeController : Controller
{
    private readonly PortfolioContentService _contentService;

    public HomeController(PortfolioContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        ViewData["FaviconHref"] = snapshot.Document.FaviconSrc;
        return View(snapshot.Document);
    }

    [HttpGet("/personal/{index:int}/{slug?}")]
    public async Task<IActionResult> Personal(int index, string? slug, CancellationToken cancellationToken)
    {
        var snapshot = await _contentService.GetSnapshotAsync(cancellationToken);
        var document = snapshot.Document;

        if (index < 0 || index >= document.PersonalSection.Items.Count)
        {
            return NotFound();
        }

        var item = document.PersonalSection.Items[index];
        var canonicalSlug = PersonalItemRouteHelper.CreateSlug(item.Title);

        if (!string.IsNullOrWhiteSpace(canonicalSlug)
            && !string.Equals(slug, canonicalSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Personal), new { index, slug = canonicalSlug })!;
        }

        if (string.IsNullOrWhiteSpace(canonicalSlug) && !string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Personal), new { index })!;
        }

        ViewData["FaviconHref"] = document.FaviconSrc;

        var model = new PersonalItemDetailViewModel
        {
            Document = document,
            Item = item,
            RelatedItems = document.PersonalSection.Items
                .Select((relatedItem, relatedIndex) => new PersonalItemPreviewViewModel
                {
                    Index = relatedIndex,
                    Slug = PersonalItemRouteHelper.CreateSlug(relatedItem.Title),
                    Category = relatedItem.Category,
                    Title = relatedItem.Title,
                    Summary = relatedItem.Summary
                })
                .Where(relatedItem => relatedItem.Index != index)
                .Take(2)
                .ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("/home/error")]
    public IActionResult Error()
    {
        return View();
    }
}
