namespace PortfolioSite.Data;

public sealed class PortfolioHighlightRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
}

public sealed class PortfolioCareerItemRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Period { get; set; } = "";
    public string Category { get; set; } = "";
    public string Organization { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Highlights { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
}

public sealed class PortfolioSkillCategoryRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
    public List<PortfolioSkillItemRecord> Items { get; set; } = [];
}

public sealed class PortfolioSkillItemRecord
{
    public int Id { get; set; }
    public int PortfolioSkillCategoryId { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = "";
    public string Experience { get; set; } = "";
    public string Note { get; set; } = "";
    public PortfolioSkillCategoryRecord PortfolioSkillCategory { get; set; } = null!;
}

public sealed class PortfolioWorkItemRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Year { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Responsibilities { get; set; } = "";
    public string Outcomes { get; set; } = "";
    public string Stack { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
}

public sealed class PortfolioPersonalItemRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string DetailSummary { get; set; } = "";
    public string DetailBody { get; set; } = "";
    public string Points { get; set; } = "";
    public string Stack { get; set; } = "";
    public string ImageSrc { get; set; } = "";
    public string ImageAlt { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
}

public sealed class PortfolioContactLinkRecord
{
    public int Id { get; set; }
    public int PortfolioContentId { get; set; }
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    public string Href { get; set; } = "";
    public PortfolioContentRecord PortfolioContent { get; set; } = null!;
}
