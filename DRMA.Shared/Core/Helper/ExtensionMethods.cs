using System.Text;

namespace DRMA.Shared.Core.Helper;

public static class ExtensionMethods
{
    public static bool Empty<TSource>(this IEnumerable<TSource> source)
    {
        return !source.Any();
    }

    public static bool Empty<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return !source.Any(predicate);
    }

    public static bool Empty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool NotEmpty<TSource>(this IEnumerable<TSource> source)
    {
        return source.Any();
    }

    public static bool NotEmpty(this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    public static string SimpleEncrypt(this string? url)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(url ?? ""));
    }

    public static string SimpleDecrypt(this string? obfuscatedUrl)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(obfuscatedUrl ?? ""));
    }
}