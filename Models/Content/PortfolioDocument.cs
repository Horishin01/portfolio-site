using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PortfolioSite.Models.Content;

public sealed class PortfolioDocument
{
    [Display(Name = "言語設定")]
    public string Locale { get; set; } = "ja";

    [Display(Name = "サイトタイトル")]
    public string SiteTitle { get; set; } = "";

    [Display(Name = "メタディスクリプション")]
    public string MetaDescription { get; set; } = "";

    [Display(Name = "ファビコンURL")]
    public string FaviconSrc { get; set; } = "";

    [Display(Name = "AdSense 設定")]
    public AdsenseContent Adsense { get; set; } = new();

    [Display(Name = "プロフィール")]
    public ProfileContent Profile { get; set; } = new();

    [Display(Name = "自己紹介セクション")]
    public ProfileSectionContent ProfileSection { get; set; } = new();

    [Display(Name = "在籍歴セクション")]
    public CareerSectionContent CareerSection { get; set; } = new();

    [Display(Name = "スキルセクション")]
    public SkillsSectionContent SkillsSection { get; set; } = new();

    [Display(Name = "実績セクション")]
    public WorksSectionContent WorksSection { get; set; } = new();

    [Display(Name = "個人活動セクション")]
    public PersonalSectionContent PersonalSection { get; set; } = new();

    [Display(Name = "連絡先")]
    public ContactContent Contact { get; set; } = new();

    [Display(Name = "フッター肩書き")]
    public string FooterRole { get; set; } = "";
}

public sealed class ProfileContent
{
    [Display(Name = "名前")]
    public string Name { get; set; } = "";

    [Display(Name = "略称")]
    public string ShortName { get; set; } = "";

    [Display(Name = "肩書")]
    public string Role { get; set; } = "";

    [Display(Name = "ヒーロー補助見出し")]
    public string HeroEyebrow { get; set; } = "";

    [Display(Name = "ヒーロー見出し")]
    public string HeroTitle { get; set; } = "";

    [Display(Name = "ヒーロー画像URL")]
    public string HeroImageSrc { get; set; } = "";

    [Display(Name = "ヒーロー画像の説明文")]
    public string HeroImageAlt { get; set; } = "";

    [Display(Name = "概要")]
    public string Summary { get; set; } = "";

    [Display(Name = "タグ")]
    public string Tags { get; set; } = "";

    [Display(Name = "ハイライト")]
    public List<LabeledValueItem> Highlights { get; set; } = [];
}

public sealed class AdsenseContent
{
    [Display(Name = "公開ページで AdSense を有効にする")]
    public bool IsEnabled { get; set; }

    [Display(Name = "Publisher ID")]
    public string PublisherId { get; set; } = "";

    [Display(Name = "head 追加コード")]
    public string HeadScript { get; set; } = "";

    [Display(Name = "body 末尾追加コード")]
    public string BodyScript { get; set; } = "";
}

public sealed class ProfileSectionContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "導入文")]
    public string Intro { get; set; } = "";

    [Display(Name = "リード文")]
    public string Lead { get; set; } = "";

    [Display(Name = "本文")]
    public string Body { get; set; } = "";

    [Display(Name = "注力分野の見出し")]
    public string FocusHeading { get; set; } = "";

    [Display(Name = "注力分野")]
    public string FocusItems { get; set; } = "";

    [Display(Name = "資格・認定の見出し")]
    public string CertificationsHeading { get; set; } = "";

    [Display(Name = "資格・認定")]
    public string Certifications { get; set; } = "";
}

public sealed class CareerSectionContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "導入文")]
    public string Intro { get; set; } = "";

    [Display(Name = "項目")]
    public List<CareerItem> Items { get; set; } = [];
}

public sealed class CareerItem
{
    private static readonly Regex PeriodDatePattern = new(
        @"(?<!\d)(\d{4})(?!\d)(?:\s*年?\s*(\d{1,2})\s*月)?",
        RegexOptions.Compiled
    );

    [Display(Name = "期間")]
    public string Period { get; set; } = "";

    [Display(Name = "開始年")]
    public string PeriodStartYear { get; set; } = "";

    [Display(Name = "開始月")]
    public string PeriodStartMonth { get; set; } = "";

    [Display(Name = "終了年")]
    public string PeriodEndYear { get; set; } = "";

    [Display(Name = "終了月")]
    public string PeriodEndMonth { get; set; } = "";

    [Display(Name = "カテゴリ")]
    public string Category { get; set; } = "";

    [Display(Name = "組織名")]
    public string Organization { get; set; } = "";

    [Display(Name = "タイトル")]
    public string Title { get; set; } = "";

    [Display(Name = "説明")]
    public string Description { get; set; } = "";

    [Display(Name = "ハイライト")]
    public string Highlights { get; set; } = "";

    public void EnsurePeriodYearFields()
    {
        Period ??= "";
        PeriodStartYear ??= "";
        PeriodStartMonth ??= "";
        PeriodEndYear ??= "";
        PeriodEndMonth ??= "";

        if (!string.IsNullOrWhiteSpace(PeriodStartYear)
            || !string.IsNullOrWhiteSpace(PeriodStartMonth)
            || !string.IsNullOrWhiteSpace(PeriodEndYear)
            || !string.IsNullOrWhiteSpace(PeriodEndMonth))
        {
            return;
        }

        var dates = PeriodDatePattern.Matches(Period)
            .Select(match => new
            {
                Year = match.Groups[1].Value,
                Month = NormalizeMonth(match.Groups[2].Value)
            })
            .ToList();

        if (dates.Count > 0)
        {
            PeriodStartYear = dates[0].Year;
            PeriodStartMonth = dates[0].Month;
        }

        if (Period.Contains("現在", StringComparison.Ordinal))
        {
            PeriodEndYear = "現在";
            return;
        }

        if (dates.Count > 1)
        {
            PeriodEndYear = dates[1].Year;
            PeriodEndMonth = dates[1].Month;
        }
    }

    public void ApplyPeriodFromYearFields()
    {
        EnsurePeriodYearFields();

        var startYear = PeriodStartYear.Trim();
        var startMonth = NormalizeMonth(PeriodStartMonth);
        var endYear = PeriodEndYear.Trim();
        var endMonth = NormalizeMonth(PeriodEndMonth);
        var now = DateTime.Now;

        Period = (startYear, endYear) switch
        {
            ({ Length: > 0 }, "現在") => $"{FormatYearMonth(startYear, startMonth)}～現在{FormatYearMonth(now.Year.ToString(), now.Month.ToString())}",
            ({ Length: > 0 }, { Length: > 0 }) => $"{FormatYearMonth(startYear, startMonth)}～{FormatYearMonth(endYear, endMonth)}",
            ({ Length: > 0 }, _) => FormatYearMonth(startYear, startMonth),
            (_, "現在") => $"現在{FormatYearMonth(now.Year.ToString(), now.Month.ToString())}",
            (_, { Length: > 0 }) => FormatYearMonth(endYear, endMonth),
            _ => Period.Trim()
        };
    }

    private static string FormatYearMonth(string year, string month)
    {
        return string.IsNullOrWhiteSpace(month)
            ? $"{year}年"
            : $"{year}年 {month}月";
    }

    private static string NormalizeMonth(string? month)
    {
        return int.TryParse(month, out var parsedMonth) && parsedMonth is >= 1 and <= 12
            ? parsedMonth.ToString()
            : "";
    }
}

public sealed class SkillsSectionContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "導入文")]
    public string Intro { get; set; } = "";

    [Display(Name = "カテゴリ")]
    public List<SkillCategory> Categories { get; set; } = [];
}

public sealed class SkillCategory
{
    [Display(Name = "カテゴリ名")]
    public string Title { get; set; } = "";

    [Display(Name = "概要")]
    public string Summary { get; set; } = "";

    [Display(Name = "スキル項目")]
    public List<SkillItem> Items { get; set; } = [];
}

public sealed class SkillItem
{
    [Display(Name = "スキル名")]
    public string Name { get; set; } = "";

    [Display(Name = "経験年数")]
    public string Experience { get; set; } = "";

    [Display(Name = "補足")]
    public string Note { get; set; } = "";
}

public sealed class WorksSectionContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "導入文")]
    public string Intro { get; set; } = "";

    [Display(Name = "実績項目")]
    public List<WorkItem> Items { get; set; } = [];
}

public sealed class WorkItem
{
    [Display(Name = "タイトル")]
    public string Title { get; set; } = "";

    [Display(Name = "年")]
    public string Year { get; set; } = "";

    [Display(Name = "種別")]
    public string Type { get; set; } = "";

    [Display(Name = "担当")]
    public string Role { get; set; } = "";

    [Display(Name = "概要")]
    public string Summary { get; set; } = "";

    [Display(Name = "担当内容")]
    public string Responsibilities { get; set; } = "";

    [Display(Name = "成果")]
    public string Outcomes { get; set; } = "";

    [Display(Name = "技術スタック")]
    public string Stack { get; set; } = "";
}

public sealed class PersonalSectionContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "導入文")]
    public string Intro { get; set; } = "";

    [Display(Name = "個人活動")]
    public List<PersonalItem> Items { get; set; } = [];
}

public sealed class PersonalItem
{
    [Display(Name = "カテゴリ")]
    public string Category { get; set; } = "";

    [Display(Name = "タイトル")]
    public string Title { get; set; } = "";

    [Display(Name = "一覧カードの概要")]
    public string Summary { get; set; } = "";

    [Display(Name = "個別ページの要約")]
    public string DetailSummary { get; set; } = "";

    [Display(Name = "個別ページ本文")]
    public string DetailBody { get; set; } = "";

    [Display(Name = "個別ページの箇条書き")]
    public string Points { get; set; } = "";

    [Display(Name = "技術スタック")]
    public string Stack { get; set; } = "";

    [Display(Name = "画像URL")]
    public string ImageSrc { get; set; } = "";

    [Display(Name = "画像の説明文")]
    public string ImageAlt { get; set; } = "";
}

public sealed class ContactContent
{
    [Display(Name = "見出し")]
    public string Heading { get; set; } = "";

    [Display(Name = "案内文")]
    public string Note { get; set; } = "";

    [Display(Name = "メールアドレス")]
    public string Email { get; set; } = "";

    [Display(Name = "リンク")]
    public List<LinkItem> Links { get; set; } = [];
}

public sealed class LinkItem
{
    [Display(Name = "リンク名")]
    public string Label { get; set; } = "";

    [Display(Name = "リンクURL")]
    public string Href { get; set; } = "";
}

public sealed class LabeledValueItem
{
    [Display(Name = "ラベル")]
    public string Label { get; set; } = "";

    [Display(Name = "値")]
    public string Value { get; set; } = "";
}
