using Microsoft.AspNetCore.Mvc;
using PortfolioSite.Services;

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
        return View(snapshot.Document);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("/home/error")]
    public IActionResult Error()
    {
        return View();
    }
}
