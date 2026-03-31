using Microsoft.EntityFrameworkCore;
using PortfolioSite.Data;
using PortfolioSite.Models.Content;

namespace PortfolioSite.Services;

public sealed class PortfolioContentService
{
    private readonly PortfolioDbContext _dbContext;

    public PortfolioContentService(PortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasContentAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PortfolioContents
            .AsNoTracking()
            .AnyAsync(content => content.Id == 1, cancellationToken);
    }

    public async Task<PortfolioSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var record = await LoadRecordAsync(asTracking: false, cancellationToken);

        if (record is null)
        {
            return await ResetToDefaultAsync(cancellationToken);
        }

        return new PortfolioSnapshot
        {
            Document = MapToDocument(record),
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task<PortfolioSnapshot> SaveAsync(PortfolioDocument document, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(document);
        var record = await LoadRecordAsync(asTracking: true, cancellationToken);

        if (record is null)
        {
            record = new PortfolioContentRecord
            {
                Id = 1
            };

            _dbContext.PortfolioContents.Add(record);
        }

        ApplyScalarFields(record, normalized);
        ReplaceProfileHighlights(record, normalized.Profile.Highlights);
        ReplaceCareerItems(record, normalized.CareerSection.Items);
        ReplaceSkillCategories(record, normalized.SkillsSection.Categories);
        ReplaceWorkItems(record, normalized.WorksSection.Items);
        ReplacePersonalItems(record, normalized.PersonalSection.Items);
        ReplaceContactLinks(record, normalized.Contact.Links);
        record.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PortfolioSnapshot
        {
            Document = MapToDocument(record),
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task<PortfolioSnapshot> ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await SaveAsync(PortfolioDefaults.Create(), cancellationToken);
    }

    private async Task<PortfolioContentRecord?> LoadRecordAsync(bool asTracking, CancellationToken cancellationToken)
    {
        IQueryable<PortfolioContentRecord> query = _dbContext.PortfolioContents
            .Include(content => content.ProfileHighlights)
            .Include(content => content.CareerItems)
            .Include(content => content.SkillCategories)
                .ThenInclude(category => category.Items)
            .Include(content => content.WorkItems)
            .Include(content => content.PersonalItems)
            .Include(content => content.ContactLinks);

        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(content => content.Id == 1, cancellationToken);
    }

    private static PortfolioDocument MapToDocument(PortfolioContentRecord record)
    {
        return Normalize(new PortfolioDocument
        {
            Locale = record.Locale,
            SiteTitle = record.SiteTitle,
            MetaDescription = record.MetaDescription,
            Profile = new ProfileContent
            {
                Name = record.ProfileName,
                ShortName = record.ProfileShortName,
                Role = record.ProfileRole,
                HeroEyebrow = record.HeroEyebrow,
                HeroTitle = record.HeroTitle,
                HeroImageSrc = record.HeroImageSrc,
                HeroImageAlt = record.HeroImageAlt,
                Summary = record.ProfileSummary,
                Tags = record.ProfileTags,
                Highlights = record.ProfileHighlights
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new LabeledValueItem
                    {
                        Label = item.Label,
                        Value = item.Value
                    })
                    .ToList()
            },
            ProfileSection = new ProfileSectionContent
            {
                Heading = record.ProfileSectionHeading,
                Intro = record.ProfileSectionIntro,
                Lead = record.ProfileSectionLead,
                Body = record.ProfileSectionBody,
                FocusHeading = record.ProfileFocusHeading,
                FocusItems = record.ProfileFocusItems,
                CertificationsHeading = record.ProfileCertificationsHeading,
                Certifications = record.ProfileCertifications
            },
            CareerSection = new CareerSectionContent
            {
                Heading = record.CareerSectionHeading,
                Intro = record.CareerSectionIntro,
                Items = record.CareerItems
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new CareerItem
                    {
                        Period = item.Period,
                        Category = item.Category,
                        Organization = item.Organization,
                        Title = item.Title,
                        Description = item.Description,
                        Highlights = item.Highlights
                    })
                    .ToList()
            },
            SkillsSection = new SkillsSectionContent
            {
                Heading = record.SkillsSectionHeading,
                Intro = record.SkillsSectionIntro,
                Categories = record.SkillCategories
                    .OrderBy(category => category.SortOrder)
                    .Select(category => new SkillCategory
                    {
                        Title = category.Title,
                        Summary = category.Summary,
                        Items = category.Items
                            .OrderBy(item => item.SortOrder)
                            .Select(item => new SkillItem
                            {
                                Name = item.Name,
                                Experience = item.Experience,
                                Note = item.Note
                            })
                            .ToList()
                    })
                    .ToList()
            },
            WorksSection = new WorksSectionContent
            {
                Heading = record.WorksSectionHeading,
                Intro = record.WorksSectionIntro,
                Items = record.WorkItems
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new WorkItem
                    {
                        Title = item.Title,
                        Year = item.Year,
                        Type = item.Type,
                        Role = item.Role,
                        Summary = item.Summary,
                        Responsibilities = item.Responsibilities,
                        Outcomes = item.Outcomes,
                        Stack = item.Stack
                    })
                    .ToList()
            },
            PersonalSection = new PersonalSectionContent
            {
                Heading = record.PersonalSectionHeading,
                Intro = record.PersonalSectionIntro,
                Items = record.PersonalItems
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new PersonalItem
                    {
                        Category = item.Category,
                        Title = item.Title,
                        Summary = item.Summary,
                        Points = item.Points,
                        Stack = item.Stack
                    })
                    .ToList()
            },
            Contact = new ContactContent
            {
                Heading = record.ContactHeading,
                Note = record.ContactNote,
                Email = record.ContactEmail,
                Links = record.ContactLinks
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new LinkItem
                    {
                        Label = item.Label,
                        Href = item.Href
                    })
                    .ToList()
            },
            FooterRole = record.FooterRole
        });
    }

    private void ReplaceProfileHighlights(PortfolioContentRecord record, IReadOnlyList<LabeledValueItem> highlights)
    {
        _dbContext.ProfileHighlights.RemoveRange(record.ProfileHighlights);
        record.ProfileHighlights.Clear();

        for (var index = 0; index < highlights.Count; index++)
        {
            var highlight = highlights[index];
            record.ProfileHighlights.Add(new PortfolioHighlightRecord
            {
                SortOrder = index,
                Label = highlight.Label,
                Value = highlight.Value
            });
        }
    }

    private void ReplaceCareerItems(PortfolioContentRecord record, IReadOnlyList<CareerItem> items)
    {
        _dbContext.CareerItems.RemoveRange(record.CareerItems);
        record.CareerItems.Clear();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            record.CareerItems.Add(new PortfolioCareerItemRecord
            {
                SortOrder = index,
                Period = item.Period,
                Category = item.Category,
                Organization = item.Organization,
                Title = item.Title,
                Description = item.Description,
                Highlights = item.Highlights
            });
        }
    }

    private void ReplaceSkillCategories(PortfolioContentRecord record, IReadOnlyList<SkillCategory> categories)
    {
        var existingItems = record.SkillCategories.SelectMany(category => category.Items).ToList();
        _dbContext.SkillItems.RemoveRange(existingItems);
        _dbContext.SkillCategories.RemoveRange(record.SkillCategories);
        record.SkillCategories.Clear();

        for (var categoryIndex = 0; categoryIndex < categories.Count; categoryIndex++)
        {
            var category = categories[categoryIndex];
            var categoryRecord = new PortfolioSkillCategoryRecord
            {
                SortOrder = categoryIndex,
                Title = category.Title,
                Summary = category.Summary
            };

            for (var itemIndex = 0; itemIndex < category.Items.Count; itemIndex++)
            {
                var item = category.Items[itemIndex];
                categoryRecord.Items.Add(new PortfolioSkillItemRecord
                {
                    SortOrder = itemIndex,
                    Name = item.Name,
                    Experience = item.Experience,
                    Note = item.Note
                });
            }

            record.SkillCategories.Add(categoryRecord);
        }
    }

    private void ReplaceWorkItems(PortfolioContentRecord record, IReadOnlyList<WorkItem> items)
    {
        _dbContext.WorkItems.RemoveRange(record.WorkItems);
        record.WorkItems.Clear();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            record.WorkItems.Add(new PortfolioWorkItemRecord
            {
                SortOrder = index,
                Title = item.Title,
                Year = item.Year,
                Type = item.Type,
                Role = item.Role,
                Summary = item.Summary,
                Responsibilities = item.Responsibilities,
                Outcomes = item.Outcomes,
                Stack = item.Stack
            });
        }
    }

    private void ReplacePersonalItems(PortfolioContentRecord record, IReadOnlyList<PersonalItem> items)
    {
        _dbContext.PersonalItems.RemoveRange(record.PersonalItems);
        record.PersonalItems.Clear();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            record.PersonalItems.Add(new PortfolioPersonalItemRecord
            {
                SortOrder = index,
                Category = item.Category,
                Title = item.Title,
                Summary = item.Summary,
                Points = item.Points,
                Stack = item.Stack
            });
        }
    }

    private void ReplaceContactLinks(PortfolioContentRecord record, IReadOnlyList<LinkItem> items)
    {
        _dbContext.ContactLinks.RemoveRange(record.ContactLinks);
        record.ContactLinks.Clear();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            record.ContactLinks.Add(new PortfolioContactLinkRecord
            {
                SortOrder = index,
                Label = item.Label,
                Href = item.Href
            });
        }
    }

    private static void ApplyScalarFields(PortfolioContentRecord record, PortfolioDocument document)
    {
        record.Locale = document.Locale;
        record.SiteTitle = document.SiteTitle;
        record.MetaDescription = document.MetaDescription;
        record.ProfileName = document.Profile.Name;
        record.ProfileShortName = document.Profile.ShortName;
        record.ProfileRole = document.Profile.Role;
        record.HeroEyebrow = document.Profile.HeroEyebrow;
        record.HeroTitle = document.Profile.HeroTitle;
        record.HeroImageSrc = document.Profile.HeroImageSrc;
        record.HeroImageAlt = document.Profile.HeroImageAlt;
        record.ProfileSummary = document.Profile.Summary;
        record.ProfileTags = document.Profile.Tags;
        record.ProfileSectionHeading = document.ProfileSection.Heading;
        record.ProfileSectionIntro = document.ProfileSection.Intro;
        record.ProfileSectionLead = document.ProfileSection.Lead;
        record.ProfileSectionBody = document.ProfileSection.Body;
        record.ProfileFocusHeading = document.ProfileSection.FocusHeading;
        record.ProfileFocusItems = document.ProfileSection.FocusItems;
        record.ProfileCertificationsHeading = document.ProfileSection.CertificationsHeading;
        record.ProfileCertifications = document.ProfileSection.Certifications;
        record.CareerSectionHeading = document.CareerSection.Heading;
        record.CareerSectionIntro = document.CareerSection.Intro;
        record.SkillsSectionHeading = document.SkillsSection.Heading;
        record.SkillsSectionIntro = document.SkillsSection.Intro;
        record.WorksSectionHeading = document.WorksSection.Heading;
        record.WorksSectionIntro = document.WorksSection.Intro;
        record.PersonalSectionHeading = document.PersonalSection.Heading;
        record.PersonalSectionIntro = document.PersonalSection.Intro;
        record.ContactHeading = document.Contact.Heading;
        record.ContactNote = document.Contact.Note;
        record.ContactEmail = document.Contact.Email;
        record.FooterRole = document.FooterRole;
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
