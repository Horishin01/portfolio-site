using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PortfolioSite.Models.Content;

namespace PortfolioSite.ViewModels;

public sealed class PortfolioEditorViewModel
{
    public PortfolioDocument Document { get; set; } = new();

    [Display(Name = "ファビコンアップロード")]
    public IFormFile? FaviconFile { get; set; }

    [Display(Name = "ヒーロー画像アップロード")]
    public IFormFile? HeroImageFile { get; set; }
    public DateTime UpdatedAtUtc { get; init; }
    public DateTime UpdatedAtJst => AdminDateTimeDisplay.ToJst(UpdatedAtUtc);

    public static PortfolioEditorViewModel FromSnapshot(PortfolioSnapshot snapshot)
    {
        return new PortfolioEditorViewModel
        {
            Document = snapshot.Document,
            UpdatedAtUtc = snapshot.UpdatedAtUtc
        };
    }

    public void Normalize()
    {
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

        foreach (var category in Document.SkillsSection.Categories)
        {
            category.Items ??= [];
        }
    }
}
