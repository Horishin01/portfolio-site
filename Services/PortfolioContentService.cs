using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PortfolioSite.Data;
using PortfolioSite.Models.Content;
using PortfolioSite.Security;

namespace PortfolioSite.Services;

public sealed class PortfolioContentService
{
    private const string SnapshotCacheKey = "portfolio.snapshot";

    private readonly IMemoryCache _cache;
    private readonly PortfolioDbContext _dbContext;

    public PortfolioContentService(PortfolioDbContext dbContext, IMemoryCache cache)
    {
        _cache = cache;
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
        if (_cache.TryGetValue<PortfolioSnapshot>(SnapshotCacheKey, out var cachedSnapshot))
        {
            return CloneSnapshot(cachedSnapshot!);
        }

        var record = await LoadRecordAsync(asTracking: false, cancellationToken);

        if (record is null)
        {
            return await ResetToDefaultAsync(cancellationToken);
        }

        var snapshot = new PortfolioSnapshot
        {
            Document = MapToDocument(record),
            UpdatedAtUtc = record.UpdatedAtUtc
        };

        CacheSnapshot(snapshot);
        return CloneSnapshot(snapshot);
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

        var snapshot = new PortfolioSnapshot
        {
            Document = MapToDocument(record),
            UpdatedAtUtc = record.UpdatedAtUtc
        };

        CacheSnapshot(snapshot);
        return CloneSnapshot(snapshot);
    }

    public async Task<PortfolioSnapshot> SaveAdsenseSettingsAsync(
        AdsenseContent adsense,
        CancellationToken cancellationToken = default
    )
    {
        var record = await LoadRecordAsync(asTracking: true, cancellationToken);

        if (record is null)
        {
            var snapshot = await ResetToDefaultAsync(cancellationToken);
            record = await LoadRecordAsync(asTracking: true, cancellationToken)
                ?? throw new InvalidOperationException("Portfolio content could not be initialized.");
        }

        record.AdsenseIsEnabled = adsense.IsEnabled;
        record.AdsensePublisherId = GoogleAdsensePublisherId.Normalize(adsense.PublisherId);
        record.AdsenseHeadScript = "";
        record.AdsenseBodyScript = "";
        record.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedSnapshot = new PortfolioSnapshot
        {
            Document = MapToDocument(record),
            UpdatedAtUtc = record.UpdatedAtUtc
        };

        CacheSnapshot(updatedSnapshot);
        return CloneSnapshot(updatedSnapshot);
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
            FaviconSrc = record.FaviconSrc,
            Adsense = new AdsenseContent
            {
                IsEnabled = record.AdsenseIsEnabled,
                PublisherId = GoogleAdsensePublisherId.Normalize(record.AdsensePublisherId),
                HeadScript = "",
                BodyScript = ""
            },
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
                        DetailSummary = item.DetailSummary,
                        DetailBody = item.DetailBody,
                        Points = item.Points,
                        Stack = item.Stack,
                        ImageSrc = item.ImageSrc,
                        ImageAlt = item.ImageAlt
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
                DetailSummary = item.DetailSummary,
                DetailBody = item.DetailBody,
                Points = item.Points,
                Stack = item.Stack,
                ImageSrc = item.ImageSrc,
                ImageAlt = item.ImageAlt
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
        record.FaviconSrc = document.FaviconSrc;
        record.AdsenseIsEnabled = document.Adsense.IsEnabled;
        record.AdsensePublisherId = GoogleAdsensePublisherId.Normalize(document.Adsense.PublisherId);
        record.AdsenseHeadScript = "";
        record.AdsenseBodyScript = "";
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
        static string EmptyIfNull(string? value) => value ?? "";

        document.Locale = string.IsNullOrWhiteSpace(document.Locale) ? "ja" : document.Locale;
        document.SiteTitle = EmptyIfNull(document.SiteTitle);
        document.MetaDescription = EmptyIfNull(document.MetaDescription);
        document.FaviconSrc = EmptyIfNull(document.FaviconSrc);
        document.FooterRole = EmptyIfNull(document.FooterRole);
        document.Adsense ??= new AdsenseContent();
        document.Adsense.PublisherId = EmptyIfNull(document.Adsense.PublisherId);
        document.Adsense.HeadScript = EmptyIfNull(document.Adsense.HeadScript);
        document.Adsense.BodyScript = EmptyIfNull(document.Adsense.BodyScript);

        document.Profile ??= new ProfileContent();
        document.Profile.Highlights ??= [];
        document.Profile.Name = EmptyIfNull(document.Profile.Name);
        document.Profile.ShortName = EmptyIfNull(document.Profile.ShortName);
        document.Profile.Role = EmptyIfNull(document.Profile.Role);
        document.Profile.HeroEyebrow = EmptyIfNull(document.Profile.HeroEyebrow);
        document.Profile.HeroTitle = EmptyIfNull(document.Profile.HeroTitle);
        document.Profile.HeroImageSrc = EmptyIfNull(document.Profile.HeroImageSrc);
        document.Profile.HeroImageAlt = EmptyIfNull(document.Profile.HeroImageAlt);
        document.Profile.Summary = EmptyIfNull(document.Profile.Summary);
        document.Profile.Tags = EmptyIfNull(document.Profile.Tags);
        foreach (var highlight in document.Profile.Highlights)
        {
            highlight.Label = EmptyIfNull(highlight.Label);
            highlight.Value = EmptyIfNull(highlight.Value);
        }

        document.ProfileSection ??= new ProfileSectionContent();
        document.ProfileSection.Heading = EmptyIfNull(document.ProfileSection.Heading);
        document.ProfileSection.Intro = EmptyIfNull(document.ProfileSection.Intro);
        document.ProfileSection.Lead = EmptyIfNull(document.ProfileSection.Lead);
        document.ProfileSection.Body = EmptyIfNull(document.ProfileSection.Body);
        document.ProfileSection.FocusHeading = EmptyIfNull(document.ProfileSection.FocusHeading);
        document.ProfileSection.FocusItems = EmptyIfNull(document.ProfileSection.FocusItems);
        document.ProfileSection.CertificationsHeading = EmptyIfNull(document.ProfileSection.CertificationsHeading);
        document.ProfileSection.Certifications = EmptyIfNull(document.ProfileSection.Certifications);

        document.CareerSection ??= new CareerSectionContent();
        document.CareerSection.Items ??= [];
        document.CareerSection.Heading = EmptyIfNull(document.CareerSection.Heading);
        document.CareerSection.Intro = EmptyIfNull(document.CareerSection.Intro);
        foreach (var item in document.CareerSection.Items)
        {
            item.Period = EmptyIfNull(item.Period);
            item.PeriodStartYear = EmptyIfNull(item.PeriodStartYear);
            item.PeriodStartMonth = EmptyIfNull(item.PeriodStartMonth);
            item.PeriodEndYear = EmptyIfNull(item.PeriodEndYear);
            item.PeriodEndMonth = EmptyIfNull(item.PeriodEndMonth);
            item.ApplyPeriodFromYearFields();
            item.Category = EmptyIfNull(item.Category);
            item.Organization = EmptyIfNull(item.Organization);
            item.Title = EmptyIfNull(item.Title);
            item.Description = EmptyIfNull(item.Description);
            item.Highlights = EmptyIfNull(item.Highlights);
        }

        document.SkillsSection ??= new SkillsSectionContent();
        document.SkillsSection.Categories ??= [];
        document.SkillsSection.Heading = EmptyIfNull(document.SkillsSection.Heading);
        document.SkillsSection.Intro = EmptyIfNull(document.SkillsSection.Intro);

        document.WorksSection ??= new WorksSectionContent();
        document.WorksSection.Items ??= [];
        document.WorksSection.Heading = EmptyIfNull(document.WorksSection.Heading);
        document.WorksSection.Intro = EmptyIfNull(document.WorksSection.Intro);
        foreach (var item in document.WorksSection.Items)
        {
            item.Title = EmptyIfNull(item.Title);
            item.Year = EmptyIfNull(item.Year);
            item.Type = EmptyIfNull(item.Type);
            item.Role = EmptyIfNull(item.Role);
            item.Summary = EmptyIfNull(item.Summary);
            item.Responsibilities = EmptyIfNull(item.Responsibilities);
            item.Outcomes = EmptyIfNull(item.Outcomes);
            item.Stack = EmptyIfNull(item.Stack);
        }

        document.PersonalSection ??= new PersonalSectionContent();
        document.PersonalSection.Items ??= [];
        document.PersonalSection.Heading = EmptyIfNull(document.PersonalSection.Heading);
        document.PersonalSection.Intro = EmptyIfNull(document.PersonalSection.Intro);
        foreach (var item in document.PersonalSection.Items)
        {
            item.Category = EmptyIfNull(item.Category);
            item.Title = EmptyIfNull(item.Title);
            item.Summary = EmptyIfNull(item.Summary);
            item.DetailSummary = EmptyIfNull(item.DetailSummary);
            item.DetailBody = EmptyIfNull(item.DetailBody);
            item.Points = EmptyIfNull(item.Points);
            item.Stack = EmptyIfNull(item.Stack);
            item.ImageSrc = EmptyIfNull(item.ImageSrc);
            item.ImageAlt = EmptyIfNull(item.ImageAlt);
        }

        document.Contact ??= new ContactContent();
        document.Contact.Links ??= [];
        document.Contact.Heading = EmptyIfNull(document.Contact.Heading);
        document.Contact.Note = EmptyIfNull(document.Contact.Note);
        document.Contact.Email = EmptyIfNull(document.Contact.Email);
        foreach (var link in document.Contact.Links)
        {
            link.Label = EmptyIfNull(link.Label);
            link.Href = EmptyIfNull(link.Href);
        }

        foreach (var category in document.SkillsSection.Categories)
        {
            category.Items ??= [];
            category.Title = EmptyIfNull(category.Title);
            category.Summary = EmptyIfNull(category.Summary);

            foreach (var item in category.Items)
            {
                item.Name = EmptyIfNull(item.Name);
                item.Experience = EmptyIfNull(item.Experience);
                item.Note = EmptyIfNull(item.Note);
            }
        }

        return document;
    }

    private void CacheSnapshot(PortfolioSnapshot snapshot)
    {
        _cache.Set(
            SnapshotCacheKey,
            snapshot,
            new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.High
            });
    }

    private static PortfolioSnapshot CloneSnapshot(PortfolioSnapshot source)
    {
        return new PortfolioSnapshot
        {
            Document = CloneDocument(source.Document),
            UpdatedAtUtc = source.UpdatedAtUtc
        };
    }

    private static PortfolioDocument CloneDocument(PortfolioDocument source)
    {
        return new PortfolioDocument
        {
            Locale = source.Locale,
            SiteTitle = source.SiteTitle,
            MetaDescription = source.MetaDescription,
            FaviconSrc = source.FaviconSrc,
            Adsense = new AdsenseContent
            {
                IsEnabled = source.Adsense.IsEnabled,
                PublisherId = source.Adsense.PublisherId,
                HeadScript = source.Adsense.HeadScript,
                BodyScript = source.Adsense.BodyScript
            },
            FooterRole = source.FooterRole,
            Profile = new ProfileContent
            {
                Name = source.Profile.Name,
                ShortName = source.Profile.ShortName,
                Role = source.Profile.Role,
                HeroEyebrow = source.Profile.HeroEyebrow,
                HeroTitle = source.Profile.HeroTitle,
                HeroImageSrc = source.Profile.HeroImageSrc,
                HeroImageAlt = source.Profile.HeroImageAlt,
                Summary = source.Profile.Summary,
                Tags = source.Profile.Tags,
                Highlights = source.Profile.Highlights
                    .Select(item => new LabeledValueItem
                    {
                        Label = item.Label,
                        Value = item.Value
                    })
                    .ToList()
            },
            ProfileSection = new ProfileSectionContent
            {
                Heading = source.ProfileSection.Heading,
                Intro = source.ProfileSection.Intro,
                Lead = source.ProfileSection.Lead,
                Body = source.ProfileSection.Body,
                FocusHeading = source.ProfileSection.FocusHeading,
                FocusItems = source.ProfileSection.FocusItems,
                CertificationsHeading = source.ProfileSection.CertificationsHeading,
                Certifications = source.ProfileSection.Certifications
            },
            CareerSection = new CareerSectionContent
            {
                Heading = source.CareerSection.Heading,
                Intro = source.CareerSection.Intro,
                Items = source.CareerSection.Items
                    .Select(item => new CareerItem
                    {
                        Period = item.Period,
                        PeriodStartYear = item.PeriodStartYear,
                        PeriodStartMonth = item.PeriodStartMonth,
                        PeriodEndYear = item.PeriodEndYear,
                        PeriodEndMonth = item.PeriodEndMonth,
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
                Heading = source.SkillsSection.Heading,
                Intro = source.SkillsSection.Intro,
                Categories = source.SkillsSection.Categories
                    .Select(category => new SkillCategory
                    {
                        Title = category.Title,
                        Summary = category.Summary,
                        Items = category.Items
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
                Heading = source.WorksSection.Heading,
                Intro = source.WorksSection.Intro,
                Items = source.WorksSection.Items
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
                Heading = source.PersonalSection.Heading,
                Intro = source.PersonalSection.Intro,
                Items = source.PersonalSection.Items
                    .Select(item => new PersonalItem
                    {
                        Category = item.Category,
                        Title = item.Title,
                        Summary = item.Summary,
                        DetailSummary = item.DetailSummary,
                        DetailBody = item.DetailBody,
                        Points = item.Points,
                        Stack = item.Stack,
                        ImageSrc = item.ImageSrc,
                        ImageAlt = item.ImageAlt
                    })
                    .ToList()
            },
            Contact = new ContactContent
            {
                Heading = source.Contact.Heading,
                Note = source.Contact.Note,
                Email = source.Contact.Email,
                Links = source.Contact.Links
                    .Select(link => new LinkItem
                    {
                        Label = link.Label,
                        Href = link.Href
                    })
                    .ToList()
            }
        };
    }

    private static string NormalizeOptionalText(string? value)
    {
        return value?.Trim() ?? "";
    }
}
