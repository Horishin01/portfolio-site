using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Data;
using PortfolioSite.Models.Content;

namespace PortfolioSite.Services;

public sealed class PortfolioContentService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly PortfolioDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public PortfolioContentService(PortfolioDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<bool> HasContentAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PortfolioContents
            .AsNoTracking()
            .AnyAsync(content => content.Id == 1, cancellationToken);
    }

    public async Task<PortfolioSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.PortfolioContents
            .AsNoTracking()
            .SingleOrDefaultAsync(content => content.Id == 1, cancellationToken);

        if (record is null)
        {
            return await ResetToSeedAsync(cancellationToken);
        }

        return new PortfolioSnapshot
        {
            Document = Deserialize(record.JsonContent),
            JsonContent = PrettyPrint(record.JsonContent),
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task<PortfolioSnapshot> SaveJsonAsync(string jsonContent, CancellationToken cancellationToken = default)
    {
        var document = Deserialize(jsonContent);
        var serialized = Serialize(document);
        var record = await _dbContext.PortfolioContents
            .SingleOrDefaultAsync(content => content.Id == 1, cancellationToken);

        if (record is null)
        {
            record = new PortfolioContentRecord
            {
                Id = 1,
                JsonContent = serialized,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.PortfolioContents.Add(record);
        }
        else
        {
            record.JsonContent = serialized;
            record.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PortfolioSnapshot
        {
            Document = document,
            JsonContent = serialized,
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task<PortfolioSnapshot> ResetToSeedAsync(CancellationToken cancellationToken = default)
    {
        var seedJson = await LoadSeedJsonAsync(cancellationToken);
        return await SaveJsonAsync(seedJson, cancellationToken);
    }

    public async Task<string> ExportJsonAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.JsonContent;
    }

    private async Task<string> LoadSeedJsonAsync(CancellationToken cancellationToken)
    {
        var seedPath = Path.Combine(_environment.ContentRootPath, "portfolio-seed.json");
        return await File.ReadAllTextAsync(seedPath, cancellationToken);
    }

    private static PortfolioDocument Deserialize(string jsonContent)
    {
        var document = JsonSerializer.Deserialize<PortfolioDocument>(jsonContent, SerializerOptions);
        if (document is null)
        {
            throw new InvalidOperationException("Portfolio JSON could not be parsed.");
        }

        return Normalize(document);
    }

    private static string Serialize(PortfolioDocument document)
    {
        return JsonSerializer.Serialize(Normalize(document), SerializerOptions);
    }

    private static string PrettyPrint(string jsonContent)
    {
        return Serialize(Deserialize(jsonContent));
    }

    private static PortfolioDocument Normalize(PortfolioDocument document)
    {
        document.Profile ??= new ProfileContent();
        document.Profile.Highlights ??= [];
        document.ProfileSection ??= new ProfileSectionContent();
        document.CareerSection ??= new CareerSectionContent();
        document.CareerSection.Items ??= [];
        document.SkillsSection ??= new SkillsSectionContent();
        document.SkillsSection.Categories ??= [];
        document.WorksSection ??= new WorksSectionContent();
        document.WorksSection.Items ??= [];
        document.PersonalSection ??= new PersonalSectionContent();
        document.PersonalSection.Items ??= [];
        document.Contact ??= new ContactContent();
        document.Contact.Links ??= [];

        foreach (var category in document.SkillsSection.Categories)
        {
            category.Items ??= [];
        }

        return document;
    }
}
