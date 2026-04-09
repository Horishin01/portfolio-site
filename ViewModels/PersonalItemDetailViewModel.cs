using PortfolioSite.Models.Content;

namespace PortfolioSite.ViewModels;

public sealed class PersonalItemDetailViewModel
{
    public PortfolioDocument Document { get; init; } = new();

    public PersonalItem Item { get; init; } = new();

    public IReadOnlyList<PersonalItemPreviewViewModel> RelatedItems { get; init; } = [];
}

public sealed class PersonalItemPreviewViewModel
{
    public int Index { get; init; }

    public string Slug { get; init; } = "";

    public string Category { get; init; } = "";

    public string Title { get; init; } = "";

    public string Summary { get; init; } = "";
}
