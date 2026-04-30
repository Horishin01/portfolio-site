namespace PortfolioSite.Security;

public static class PublicUrlSanitizer
{
    private static readonly HashSet<string> AllowedLinkSchemes =
    [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        "mailto",
        "tel"
    ];

    private static readonly HashSet<string> AllowedFaviconExtensions =
    [
        ".ico",
        ".png",
        ".webp",
        ".jpg",
        ".jpeg",
        ".gif"
    ];
    private static readonly HashSet<string> AllowedMediaSchemes =
    [
        Uri.UriSchemeHttps
    ];

    public static string? SanitizeLinkUrl(string? value)
    {
        return TrySanitizeUrl(value, AllowedLinkSchemes, pathValidator: null, out var sanitized)
            ? sanitized
            : null;
    }

    public static string? SanitizeImageUrl(string? value)
    {
        return TrySanitizeUrl(
            value,
            AllowedMediaSchemes,
            pathValidator: null,
            out var sanitized
        )
            ? sanitized
            : null;
    }

    public static string? SanitizeFaviconUrl(string? value)
    {
        return TrySanitizeUrl(
            value,
            AllowedMediaSchemes,
            HasAllowedFaviconExtension,
            out var sanitized
        )
            ? sanitized
            : null;
    }

    public static string SanitizeFaviconUrlOrDefault(string? value, string fallback)
    {
        return SanitizeFaviconUrl(value) ?? fallback;
    }

    public static bool IsExternalHttpLink(string? value)
    {
        var sanitized = SanitizeLinkUrl(value);
        return Uri.TryCreate(sanitized, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool TrySanitizeUrl(
        string? value,
        IReadOnlySet<string> allowedSchemes,
        Func<string, bool>? pathValidator,
        out string? sanitized
    )
    {
        sanitized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            sanitized = "";
            return true;
        }

        var trimmed = value.Trim();
        if (ContainsControlCharacter(trimmed))
        {
            return false;
        }

        if (trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.Contains('\\'))
            {
                return false;
            }

            var path = GetPathComponent(trimmed);
            if (pathValidator is not null && !pathValidator(path))
            {
                return false;
            }

            sanitized = trimmed;
            return true;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
        {
            return false;
        }

        if (!allowedSchemes.Contains(absoluteUri.Scheme))
        {
            return false;
        }

        if (pathValidator is not null && !pathValidator(absoluteUri.AbsolutePath))
        {
            return false;
        }

        sanitized = trimmed;
        return true;
    }

    private static bool ContainsControlCharacter(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetPathComponent(string value)
    {
        var delimiterIndex = value.IndexOfAny(new[] { '?', '#' });
        return delimiterIndex >= 0 ? value[..delimiterIndex] : value;
    }

    private static bool HasAllowedFaviconExtension(string path)
    {
        var decodedPath = Uri.UnescapeDataString(path);
        var extension = Path.GetExtension(decodedPath);
        return AllowedFaviconExtensions.Contains(extension);
    }
}
