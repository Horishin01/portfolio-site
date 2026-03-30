using Microsoft.EntityFrameworkCore;

using PortfolioSite.Data;

namespace PortfolioSite.Services;

public sealed class PortfolioDbInitializer
{
    private readonly PortfolioContentService _contentService;
    private readonly PortfolioDbContext _dbContext;

    public PortfolioDbInitializer(PortfolioContentService contentService, PortfolioDbContext dbContext)
    {
        _contentService = contentService;
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        if (!await _contentService.HasContentAsync(cancellationToken))
        {
            await _contentService.ResetToSeedAsync(cancellationToken);
        }
    }
}
