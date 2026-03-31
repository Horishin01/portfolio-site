using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioSite.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePortfolioContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareerSectionHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CareerSectionIntro",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactNote",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FooterRole",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeroEyebrow",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeroImageAlt",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeroImageSrc",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PersonalSectionHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PersonalSectionIntro",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileCertifications",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileCertificationsHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileFocusHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileFocusItems",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileRole",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSectionBody",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSectionHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSectionIntro",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSectionLead",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileShortName",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSummary",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProfileTags",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SiteTitle",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SkillsSectionHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SkillsSectionIntro",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorksSectionHeading",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WorksSectionIntro",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_career_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Organization = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Highlights = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_career_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_career_items_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_contact_links",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Href = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_contact_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_contact_links_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_personal_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Points = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stack = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_personal_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_personal_items_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_profile_highlights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_profile_highlights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_profile_highlights_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_skill_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_skill_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_skill_categories_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_work_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Summary = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Responsibilities = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Outcomes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stack = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_work_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_work_items_portfolio_contents_PortfolioContentId",
                        column: x => x.PortfolioContentId,
                        principalTable: "portfolio_contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "portfolio_skill_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortfolioSkillCategoryId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Experience = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_skill_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_skill_items_portfolio_skill_categories_PortfolioSkillCategoryId",
                        column: x => x.PortfolioSkillCategoryId,
                        principalTable: "portfolio_skill_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_career_items_PortfolioContentId",
                table: "portfolio_career_items",
                column: "PortfolioContentId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_contact_links_PortfolioContentId",
                table: "portfolio_contact_links",
                column: "PortfolioContentId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_personal_items_PortfolioContentId",
                table: "portfolio_personal_items",
                column: "PortfolioContentId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_profile_highlights_PortfolioContentId",
                table: "portfolio_profile_highlights",
                column: "PortfolioContentId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_skill_categories_PortfolioContentId",
                table: "portfolio_skill_categories",
                column: "PortfolioContentId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_skill_items_PortfolioSkillCategoryId",
                table: "portfolio_skill_items",
                column: "PortfolioSkillCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_portfolio_work_items_PortfolioContentId",
                table: "portfolio_work_items",
                column: "PortfolioContentId");

            if (ActiveProvider.Contains("MySql", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.Sql(
                    """
                    UPDATE portfolio_contents
                    SET
                        Locale = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Locale')), ''),
                        SiteTitle = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.SiteTitle')), ''),
                        MetaDescription = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.MetaDescription')), ''),
                        ProfileName = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.Name')), ''),
                        ProfileShortName = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.ShortName')), ''),
                        ProfileRole = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.Role')), ''),
                        HeroEyebrow = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.HeroEyebrow')), ''),
                        HeroTitle = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.HeroTitle')), ''),
                        HeroImageSrc = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.HeroImageSrc')), ''),
                        HeroImageAlt = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.HeroImageAlt')), ''),
                        ProfileSummary = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.Summary')), ''),
                        ProfileTags = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Profile.Tags')), ''),
                        ProfileSectionHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.Heading')), ''),
                        ProfileSectionIntro = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.Intro')), ''),
                        ProfileSectionLead = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.Lead')), ''),
                        ProfileSectionBody = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.Body')), ''),
                        ProfileFocusHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.FocusHeading')), ''),
                        ProfileFocusItems = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.FocusItems')), ''),
                        ProfileCertificationsHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.CertificationsHeading')), ''),
                        ProfileCertifications = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.ProfileSection.Certifications')), ''),
                        CareerSectionHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.CareerSection.Heading')), ''),
                        CareerSectionIntro = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.CareerSection.Intro')), ''),
                        SkillsSectionHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.SkillsSection.Heading')), ''),
                        SkillsSectionIntro = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.SkillsSection.Intro')), ''),
                        WorksSectionHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.WorksSection.Heading')), ''),
                        WorksSectionIntro = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.WorksSection.Intro')), ''),
                        PersonalSectionHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.PersonalSection.Heading')), ''),
                        PersonalSectionIntro = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.PersonalSection.Intro')), ''),
                        ContactHeading = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Contact.Heading')), ''),
                        ContactNote = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Contact.Note')), ''),
                        ContactEmail = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.Contact.Email')), ''),
                        FooterRole = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(JsonContent, '$.FooterRole')), '')
                    WHERE JSON_VALID(JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_profile_highlights (PortfolioContentId, SortOrder, Label, Value)
                    SELECT
                        content.Id,
                        highlight.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(highlight.HighlightJson, '$.Label')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(highlight.HighlightJson, '$.Value')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.Profile.Highlights[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            HighlightJson JSON PATH '$'
                        )
                    ) AS highlight
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_career_items (PortfolioContentId, SortOrder, Period, Category, Organization, Title, Description, Highlights)
                    SELECT
                        content.Id,
                        item.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Period')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Category')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Organization')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Title')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Description')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Highlights')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.CareerSection.Items[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            ItemJson JSON PATH '$'
                        )
                    ) AS item
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_skill_categories (PortfolioContentId, SortOrder, Title, Summary)
                    SELECT
                        content.Id,
                        category.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(category.CategoryJson, '$.Title')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(category.CategoryJson, '$.Summary')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.SkillsSection.Categories[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            CategoryJson JSON PATH '$'
                        )
                    ) AS category
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_skill_items (PortfolioSkillCategoryId, SortOrder, Name, Experience, Note)
                    SELECT
                        categoryRecord.Id,
                        item.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Name')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Experience')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Note')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.SkillsSection.Categories[*]'
                        COLUMNS (
                            CategorySortOrder FOR ORDINALITY,
                            ItemsJson JSON PATH '$.Items'
                        )
                    ) AS category
                    JOIN portfolio_skill_categories AS categoryRecord
                        ON categoryRecord.PortfolioContentId = content.Id
                       AND categoryRecord.SortOrder = category.CategorySortOrder - 1
                    CROSS JOIN JSON_TABLE(
                        category.ItemsJson,
                        '$[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            ItemJson JSON PATH '$'
                        )
                    ) AS item
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_work_items (PortfolioContentId, SortOrder, Title, Year, Type, Role, Summary, Responsibilities, Outcomes, Stack)
                    SELECT
                        content.Id,
                        item.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Title')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Year')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Type')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Role')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Summary')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Responsibilities')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Outcomes')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Stack')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.WorksSection.Items[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            ItemJson JSON PATH '$'
                        )
                    ) AS item
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_personal_items (PortfolioContentId, SortOrder, Category, Title, Summary, Points, Stack)
                    SELECT
                        content.Id,
                        item.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Category')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Title')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Summary')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Points')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Stack')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.PersonalSection.Items[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            ItemJson JSON PATH '$'
                        )
                    ) AS item
                    WHERE JSON_VALID(content.JsonContent);
                    """);

                migrationBuilder.Sql(
                    """
                    INSERT INTO portfolio_contact_links (PortfolioContentId, SortOrder, Label, Href)
                    SELECT
                        content.Id,
                        item.SortOrder - 1,
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Label')), ''),
                        COALESCE(JSON_UNQUOTE(JSON_EXTRACT(item.ItemJson, '$.Href')), '')
                    FROM portfolio_contents AS content
                    CROSS JOIN JSON_TABLE(
                        content.JsonContent,
                        '$.Contact.Links[*]'
                        COLUMNS (
                            SortOrder FOR ORDINALITY,
                            ItemJson JSON PATH '$'
                        )
                    ) AS item
                    WHERE JSON_VALID(content.JsonContent);
                    """);
            }

            migrationBuilder.DropColumn(
                name: "JsonContent",
                table: "portfolio_contents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonContent",
                table: "portfolio_contents",
                type: "longtext",
                nullable: false,
                defaultValue: "{}")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.DropTable(
                name: "portfolio_career_items");

            migrationBuilder.DropTable(
                name: "portfolio_contact_links");

            migrationBuilder.DropTable(
                name: "portfolio_personal_items");

            migrationBuilder.DropTable(
                name: "portfolio_profile_highlights");

            migrationBuilder.DropTable(
                name: "portfolio_skill_items");

            migrationBuilder.DropTable(
                name: "portfolio_work_items");

            migrationBuilder.DropTable(
                name: "portfolio_skill_categories");

            migrationBuilder.DropColumn(
                name: "CareerSectionHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "CareerSectionIntro",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ContactHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ContactNote",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "FooterRole",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "HeroEyebrow",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "HeroImageAlt",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "HeroImageSrc",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "PersonalSectionHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "PersonalSectionIntro",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileCertifications",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileCertificationsHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileFocusHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileFocusItems",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileRole",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileSectionBody",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileSectionHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileSectionIntro",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileSectionLead",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileShortName",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileSummary",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "ProfileTags",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "SiteTitle",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "SkillsSectionHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "SkillsSectionIntro",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "WorksSectionHeading",
                table: "portfolio_contents");

            migrationBuilder.DropColumn(
                name: "WorksSectionIntro",
                table: "portfolio_contents");
        }
    }
}
