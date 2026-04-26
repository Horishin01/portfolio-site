using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Models.Identity;

namespace PortfolioSite.Data;

public sealed class PortfolioDbContext : IdentityDbContext<AdminUser>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<PortfolioContentRecord> PortfolioContents => Set<PortfolioContentRecord>();
    public DbSet<PortfolioHighlightRecord> ProfileHighlights => Set<PortfolioHighlightRecord>();
    public DbSet<PortfolioCareerItemRecord> CareerItems => Set<PortfolioCareerItemRecord>();
    public DbSet<PortfolioSkillCategoryRecord> SkillCategories => Set<PortfolioSkillCategoryRecord>();
    public DbSet<PortfolioSkillItemRecord> SkillItems => Set<PortfolioSkillItemRecord>();
    public DbSet<PortfolioWorkItemRecord> WorkItems => Set<PortfolioWorkItemRecord>();
    public DbSet<PortfolioPersonalItemRecord> PersonalItems => Set<PortfolioPersonalItemRecord>();
    public DbSet<PortfolioContactLinkRecord> ContactLinks => Set<PortfolioContactLinkRecord>();
    public DbSet<AdminAdsenseConnectionRecord> AdminAdsenseConnections => Set<AdminAdsenseConnectionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortfolioContentRecord>(entity =>
        {
            entity.ToTable("portfolio_contents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Locale).IsRequired();
            entity.Property(x => x.SiteTitle).IsRequired();
            entity.Property(x => x.MetaDescription).IsRequired();
            entity.Property(x => x.FaviconSrc).IsRequired();
            entity.Property(x => x.AdsenseIsEnabled).IsRequired();
            entity.Property(x => x.AdsensePublisherId).IsRequired();
            entity.Property(x => x.AdsenseHeadScript).IsRequired();
            entity.Property(x => x.AdsenseBodyScript).IsRequired();
            entity.Property(x => x.ProfileName).IsRequired();
            entity.Property(x => x.ProfileShortName).IsRequired();
            entity.Property(x => x.ProfileRole).IsRequired();
            entity.Property(x => x.HeroEyebrow).IsRequired();
            entity.Property(x => x.HeroTitle).IsRequired();
            entity.Property(x => x.HeroImageSrc).IsRequired();
            entity.Property(x => x.HeroImageAlt).IsRequired();
            entity.Property(x => x.ProfileSummary).IsRequired();
            entity.Property(x => x.ProfileTags).IsRequired();
            entity.Property(x => x.ProfileSectionHeading).IsRequired();
            entity.Property(x => x.ProfileSectionIntro).IsRequired();
            entity.Property(x => x.ProfileSectionLead).IsRequired();
            entity.Property(x => x.ProfileSectionBody).IsRequired();
            entity.Property(x => x.ProfileFocusHeading).IsRequired();
            entity.Property(x => x.ProfileFocusItems).IsRequired();
            entity.Property(x => x.ProfileCertificationsHeading).IsRequired();
            entity.Property(x => x.ProfileCertifications).IsRequired();
            entity.Property(x => x.CareerSectionHeading).IsRequired();
            entity.Property(x => x.CareerSectionIntro).IsRequired();
            entity.Property(x => x.SkillsSectionHeading).IsRequired();
            entity.Property(x => x.SkillsSectionIntro).IsRequired();
            entity.Property(x => x.WorksSectionHeading).IsRequired();
            entity.Property(x => x.WorksSectionIntro).IsRequired();
            entity.Property(x => x.PersonalSectionHeading).IsRequired();
            entity.Property(x => x.PersonalSectionIntro).IsRequired();
            entity.Property(x => x.ContactHeading).IsRequired();
            entity.Property(x => x.ContactNote).IsRequired();
            entity.Property(x => x.ContactEmail).IsRequired();
            entity.Property(x => x.FooterRole).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<AdminAdsenseConnectionRecord>(entity =>
        {
            entity.ToTable("admin_adsense_connections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.AccountName).IsRequired();
            entity.Property(x => x.AccountDisplayName).IsRequired();
            entity.Property(x => x.RefreshTokenCiphertext).IsRequired();
            entity.Property(x => x.ConnectedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<PortfolioHighlightRecord>(entity =>
        {
            entity.ToTable("portfolio_profile_highlights");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.ProfileHighlights)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioCareerItemRecord>(entity =>
        {
            entity.ToTable("portfolio_career_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Period).IsRequired();
            entity.Property(x => x.Category).IsRequired();
            entity.Property(x => x.Organization).IsRequired();
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.Highlights).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.CareerItems)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioSkillCategoryRecord>(entity =>
        {
            entity.ToTable("portfolio_skill_categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Summary).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.SkillCategories)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioSkillItemRecord>(entity =>
        {
            entity.ToTable("portfolio_skill_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Experience).IsRequired();
            entity.Property(x => x.Note).IsRequired();
            entity.HasOne(x => x.PortfolioSkillCategory)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PortfolioSkillCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioWorkItemRecord>(entity =>
        {
            entity.ToTable("portfolio_work_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Year).IsRequired();
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.Summary).IsRequired();
            entity.Property(x => x.Responsibilities).IsRequired();
            entity.Property(x => x.Outcomes).IsRequired();
            entity.Property(x => x.Stack).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.WorkItems)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioPersonalItemRecord>(entity =>
        {
            entity.ToTable("portfolio_personal_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).IsRequired();
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Summary).IsRequired();
            entity.Property(x => x.DetailSummary).IsRequired();
            entity.Property(x => x.DetailBody).IsRequired();
            entity.Property(x => x.Points).IsRequired();
            entity.Property(x => x.Stack).IsRequired();
            entity.Property(x => x.ImageSrc).IsRequired();
            entity.Property(x => x.ImageAlt).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.PersonalItems)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PortfolioContactLinkRecord>(entity =>
        {
            entity.ToTable("portfolio_contact_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Href).IsRequired();
            entity.HasOne(x => x.PortfolioContent)
                .WithMany(x => x.ContactLinks)
                .HasForeignKey(x => x.PortfolioContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
