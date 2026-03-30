namespace PortfolioSite.Models.Content;

public sealed class PortfolioDocument
{
    public string Locale { get; set; } = "ja";
    public string SiteTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public ProfileContent Profile { get; set; } = new();
    public ProfileSectionContent ProfileSection { get; set; } = new();
    public CareerSectionContent CareerSection { get; set; } = new();
    public SkillsSectionContent SkillsSection { get; set; } = new();
    public WorksSectionContent WorksSection { get; set; } = new();
    public PersonalSectionContent PersonalSection { get; set; } = new();
    public ContactContent Contact { get; set; } = new();
    public string FooterRole { get; set; } = "";
}

public sealed class ProfileContent
{
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Role { get; set; } = "";
    public string HeroEyebrow { get; set; } = "";
    public string HeroTitle { get; set; } = "";
    public string HeroImageSrc { get; set; } = "";
    public string HeroImageAlt { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Tags { get; set; } = "";
    public List<LabeledValueItem> Highlights { get; set; } = [];
}

public sealed class ProfileSectionContent
{
    public string Heading { get; set; } = "";
    public string Intro { get; set; } = "";
    public string Lead { get; set; } = "";
    public string Body { get; set; } = "";
    public string FocusHeading { get; set; } = "";
    public string FocusItems { get; set; } = "";
    public string CertificationsHeading { get; set; } = "";
    public string Certifications { get; set; } = "";
}

public sealed class CareerSectionContent
{
    public string Heading { get; set; } = "";
    public string Intro { get; set; } = "";
    public List<CareerItem> Items { get; set; } = [];
}

public sealed class CareerItem
{
    public string Period { get; set; } = "";
    public string Category { get; set; } = "";
    public string Organization { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Highlights { get; set; } = "";
}

public sealed class SkillsSectionContent
{
    public string Heading { get; set; } = "";
    public string Intro { get; set; } = "";
    public List<SkillCategory> Categories { get; set; } = [];
}

public sealed class SkillCategory
{
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<SkillItem> Items { get; set; } = [];
}

public sealed class SkillItem
{
    public string Name { get; set; } = "";
    public string Experience { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class WorksSectionContent
{
    public string Heading { get; set; } = "";
    public string Intro { get; set; } = "";
    public List<WorkItem> Items { get; set; } = [];
}

public sealed class WorkItem
{
    public string Title { get; set; } = "";
    public string Year { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Responsibilities { get; set; } = "";
    public string Outcomes { get; set; } = "";
    public string Stack { get; set; } = "";
}

public sealed class PersonalSectionContent
{
    public string Heading { get; set; } = "";
    public string Intro { get; set; } = "";
    public List<PersonalItem> Items { get; set; } = [];
}

public sealed class PersonalItem
{
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Points { get; set; } = "";
    public string Stack { get; set; } = "";
}

public sealed class ContactContent
{
    public string Heading { get; set; } = "";
    public string Note { get; set; } = "";
    public string Email { get; set; } = "";
    public List<LinkItem> Links { get; set; } = [];
}

public sealed class LinkItem
{
    public string Label { get; set; } = "";
    public string Href { get; set; } = "";
}

public sealed class LabeledValueItem
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
}
