using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PortfolioSite.ViewModels;

public static class ValidationMessageLocalizer
{
    private static readonly Regex RequiredPattern =
        new(@"^The (?<field>.+?) field is required\.$|^The field (?<field2>.+?) is required\.$", RegexOptions.Compiled);

    private static readonly Regex MinLengthPattern =
        new(@"^The field (?<field>.+?) must be a string or array type with a minimum length of '(?<min>\d+)'\.$", RegexOptions.Compiled);

    private static readonly Regex MaxLengthPattern =
        new(@"^The field (?<field>.+?) must be a string with a maximum length of '(?<max>\d+)'\.$", RegexOptions.Compiled);

    private static readonly Regex RangeLengthPattern =
        new(@"^The field (?<field>.+?) must be a string or array type with a minimum length of '(?<min>\d+)' and a maximum length of '(?<max>\d+)'\.$", RegexOptions.Compiled);

    private static readonly Regex EmailPattern =
        new(@"^The (?<field>.+?) field is not a valid e-mail address\.$", RegexOptions.Compiled);

    private static readonly Regex UrlPattern =
        new(@"^The (?<field>.+?) field is not a valid fully-qualified http, https, or ftp URL\.$", RegexOptions.Compiled);

    private static readonly Regex InvalidValueForFieldPattern =
        new(@"^The value '.*' is not valid for (?<field>.+?)\.$", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LoginId"] = "ID",
        ["Password"] = "パスワード",
        ["FaviconFile"] = "ファビコン",
        ["HeroImageFile"] = "ヒーロー画像",
        ["Locale"] = "言語設定",
        ["SiteTitle"] = "サイトタイトル",
        ["MetaDescription"] = "メタディスクリプション",
        ["FaviconSrc"] = "ファビコンURL",
        ["FooterRole"] = "フッター肩書き",
        ["Name"] = "名前",
        ["ShortName"] = "略称",
        ["Role"] = "肩書",
        ["HeroEyebrow"] = "ヒーロー補助見出し",
        ["HeroTitle"] = "ヒーロー見出し",
        ["HeroImageSrc"] = "ヒーロー画像URL",
        ["HeroImageAlt"] = "ヒーロー画像の説明文",
        ["Summary"] = "概要",
        ["Tags"] = "タグ",
        ["Heading"] = "見出し",
        ["Intro"] = "導入文",
        ["Lead"] = "リード文",
        ["Body"] = "本文",
        ["FocusHeading"] = "注力分野の見出し",
        ["FocusItems"] = "注力分野",
        ["CertificationsHeading"] = "資格・認定の見出し",
        ["Certifications"] = "資格・認定",
        ["Period"] = "期間",
        ["PeriodStartYear"] = "開始年",
        ["PeriodStartMonth"] = "開始月",
        ["PeriodEndYear"] = "終了年",
        ["PeriodEndMonth"] = "終了月",
        ["Category"] = "カテゴリ",
        ["Organization"] = "組織名",
        ["Title"] = "タイトル",
        ["Description"] = "説明",
        ["Highlights"] = "ハイライト",
        ["Label"] = "ラベル",
        ["Value"] = "値",
        ["Experience"] = "経験年数",
        ["Note"] = "補足",
        ["Year"] = "年",
        ["Type"] = "種別",
        ["Responsibilities"] = "担当内容",
        ["Outcomes"] = "成果",
        ["Stack"] = "技術スタック",
        ["Points"] = "内容",
        ["Email"] = "メールアドレス",
        ["Href"] = "リンクURL"
    };

    public static string? GetFirstError(ModelStateDictionary modelState)
    {
        foreach (var entry in modelState)
        {
            var error = entry.Value?.Errors.FirstOrDefault();
            if (error is null)
            {
                continue;
            }

            return ToJapanese(entry.Key, error.ErrorMessage);
        }

        return null;
    }

    public static string ToJapanese(string? modelKey, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "入力内容を確認してください。";
        }

        var fieldLabel = ResolveFieldLabel(modelKey, message);

        var rangeLengthMatch = RangeLengthPattern.Match(message);
        if (rangeLengthMatch.Success)
        {
            return $"{fieldLabel}は{rangeLengthMatch.Groups["min"].Value}文字以上、{rangeLengthMatch.Groups["max"].Value}文字以下で入力してください。";
        }

        var requiredMatch = RequiredPattern.Match(message);
        if (requiredMatch.Success)
        {
            return $"{fieldLabel}を入力してください。";
        }

        var minLengthMatch = MinLengthPattern.Match(message);
        if (minLengthMatch.Success)
        {
            return $"{fieldLabel}は{minLengthMatch.Groups["min"].Value}文字以上で入力してください。";
        }

        var maxLengthMatch = MaxLengthPattern.Match(message);
        if (maxLengthMatch.Success)
        {
            return $"{fieldLabel}は{maxLengthMatch.Groups["max"].Value}文字以下で入力してください。";
        }

        if (EmailPattern.IsMatch(message))
        {
            return $"{fieldLabel}の形式が正しくありません。";
        }

        if (UrlPattern.IsMatch(message))
        {
            return $"{fieldLabel}の URL 形式が正しくありません。";
        }

        if (InvalidValueForFieldPattern.IsMatch(message) || string.Equals(message, "The value '' is invalid.", StringComparison.Ordinal))
        {
            return $"{fieldLabel}の値が正しくありません。";
        }

        if (ContainsJapanese(message))
        {
            return message;
        }

        return message;
    }

    private static string ResolveFieldLabel(string? modelKey, string message)
    {
        foreach (var candidate in GetFieldCandidates(modelKey, message))
        {
            if (FieldLabels.TryGetValue(candidate, out var label))
            {
                return label;
            }
        }

        return ExtractFieldName(message) ?? "入力内容";
    }

    private static IEnumerable<string> GetFieldCandidates(string? modelKey, string message)
    {
        if (!string.IsNullOrWhiteSpace(modelKey))
        {
            yield return modelKey;

            var segments = modelKey
                .Replace("[", ".", StringComparison.Ordinal)
                .Replace("]", "", StringComparison.Ordinal)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);

            for (var i = segments.Length - 1; i >= 0; i--)
            {
                yield return segments[i];
            }
        }

        var fieldName = ExtractFieldName(message);
        if (!string.IsNullOrWhiteSpace(fieldName))
        {
            yield return fieldName;
        }
    }

    private static string? ExtractFieldName(string message)
    {
        return ExtractCapturedField(RequiredPattern.Match(message))
            ?? ExtractCapturedField(RangeLengthPattern.Match(message))
            ?? ExtractCapturedField(MinLengthPattern.Match(message))
            ?? ExtractCapturedField(MaxLengthPattern.Match(message))
            ?? ExtractCapturedField(EmailPattern.Match(message))
            ?? ExtractCapturedField(UrlPattern.Match(message))
            ?? ExtractCapturedField(InvalidValueForFieldPattern.Match(message));
    }

    private static string? ExtractCapturedField(Match match)
    {
        if (!match.Success)
        {
            return null;
        }

        var first = match.Groups["field"].Value;
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        var second = match.Groups["field2"].Value;
        return string.IsNullOrWhiteSpace(second) ? null : second;
    }

    private static bool ContainsJapanese(string message)
    {
        foreach (var c in message)
        {
            if ((c >= '\u3040' && c <= '\u30ff') || (c >= '\u4e00' && c <= '\u9fff'))
            {
                return true;
            }
        }

        return false;
    }
}
