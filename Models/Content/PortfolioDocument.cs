using System.ComponentModel.DataAnnotations;

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
    [Display(Name = "期間")]
    public string Period { get; set; } = "";

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
