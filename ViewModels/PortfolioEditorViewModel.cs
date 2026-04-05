using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PortfolioSite.Models.Content;

namespace PortfolioSite.ViewModels;

public sealed class PortfolioEditorViewModel
{
    public PortfolioDocument Document { get; set; } = new();
    public List<string> ProfileTagEntries { get; set; } = [];
    public List<string> CertificationEntries { get; set; } = [];

    [Display(Name = "ファビコンアップロード")]
    public IFormFile? FaviconFile { get; set; }

    [Display(Name = "ヒーロー画像アップロード")]
    public IFormFile? HeroImageFile { get; set; }
    public DateTime UpdatedAtUtc { get; init; }
    public DateTime UpdatedAtJst => AdminDateTimeDisplay.ToJst(UpdatedAtUtc);

    public static PortfolioEditorViewModel FromSnapshot(PortfolioSnapshot snapshot)
    {
        var model = new PortfolioEditorViewModel
        {
            Document = snapshot.Document,
            UpdatedAtUtc = snapshot.UpdatedAtUtc
        };

        model.LoadStructuredEntriesFromDocument();
        return model;
    }

    public void Normalize()
    {
        static List<string> SplitLines(string? value)
        {
            return (value ?? string.Empty)
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.TrimStart('-', '*', '+', ' '))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();
        }

        Document ??= new PortfolioDocument();
        Document.Profile ??= new ProfileContent();
        Document.Profile.Highlights ??= [];
        Document.ProfileSection ??= new ProfileSectionContent();
        Document.CareerSection ??= new CareerSectionContent();
        Document.CareerSection.Items ??= [];
        Document.SkillsSection ??= new SkillsSectionContent();
        Document.SkillsSection.Categories ??= [];
        Document.WorksSection ??= new WorksSectionContent();
        Document.WorksSection.Items ??= [];
        Document.PersonalSection ??= new PersonalSectionContent();
        Document.PersonalSection.Items ??= [];
        Document.Contact ??= new ContactContent();
        Document.Contact.Links ??= [];
        ProfileTagEntries ??= SplitLines(Document.Profile.Tags);
        CertificationEntries ??= SplitLines(Document.ProfileSection.Certifications);

        foreach (var category in Document.SkillsSection.Categories)
        {
            category.Items ??= [];
        }
    }

    public void LoadStructuredEntriesFromDocument()
    {
        Normalize();
        ProfileTagEntries = SplitLines(Document.Profile.Tags);
        CertificationEntries = SplitLines(Document.ProfileSection.Certifications);
    }

    public void ApplyStructuredEntriesToDocument()
    {
        Normalize();
        Document.Profile.Tags = JoinLines(ProfileTagEntries);
        Document.ProfileSection.Certifications = JoinLines(CertificationEntries);
    }

    private static List<string> SplitLines(string? value)
    {
        return (value ?? string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.TrimStart('-', '*', '+', ' '))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static string JoinLines(IEnumerable<string>? entries)
    {
        return string.Join(
            "\n",
            (entries ?? [])
                .Select(item => item?.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
        );
    }
}
