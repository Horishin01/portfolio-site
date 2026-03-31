namespace PortfolioSite.Data;

public sealed class PortfolioContentRecord
{
    public int Id { get; set; } = 1;
    public string Locale { get; set; } = "ja";
    public string SiteTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string FaviconSrc { get; set; } = "";
    public string ProfileName { get; set; } = "";
    public string ProfileShortName { get; set; } = "";
    public string ProfileRole { get; set; } = "";
    public string HeroEyebrow { get; set; } = "";
    public string HeroTitle { get; set; } = "";
    public string HeroImageSrc { get; set; } = "";
    public string HeroImageAlt { get; set; } = "";
    public string ProfileSummary { get; set; } = "";
    public string ProfileTags { get; set; } = "";
    public string ProfileSectionHeading { get; set; } = "";
    public string ProfileSectionIntro { get; set; } = "";
    public string ProfileSectionLead { get; set; } = "";
    public string ProfileSectionBody { get; set; } = "";
    public string ProfileFocusHeading { get; set; } = "";
    public string ProfileFocusItems { get; set; } = "";
    public string ProfileCertificationsHeading { get; set; } = "";
    public string ProfileCertifications { get; set; } = "";
    public string CareerSectionHeading { get; set; } = "";
    public string CareerSectionIntro { get; set; } = "";
    public string SkillsSectionHeading { get; set; } = "";
    public string SkillsSectionIntro { get; set; } = "";
    public string WorksSectionHeading { get; set; } = "";
    public string WorksSectionIntro { get; set; } = "";
    public string PersonalSectionHeading { get; set; } = "";
    public string PersonalSectionIntro { get; set; } = "";
    public string ContactHeading { get; set; } = "";
    public string ContactNote { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string FooterRole { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<PortfolioHighlightRecord> ProfileHighlights { get; set; } = [];
    public List<PortfolioCareerItemRecord> CareerItems { get; set; } = [];
    public List<PortfolioSkillCategoryRecord> SkillCategories { get; set; } = [];
    public List<PortfolioWorkItemRecord> WorkItems { get; set; } = [];
    public List<PortfolioPersonalItemRecord> PersonalItems { get; set; } = [];
    public List<PortfolioContactLinkRecord> ContactLinks { get; set; } = [];
}
