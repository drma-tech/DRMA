using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

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

    public static bool Empty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool NotEmpty<TSource>(this IEnumerable<TSource> source)
    {
        return source.Any();
    }

    public static bool NotEmpty([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    public static string SimpleEncrypt(this string? value)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));

        return base64
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public static string SimpleDecrypt(this string? encoded)
    {
        if (string.IsNullOrEmpty(encoded))
            return string.Empty;

        string padded = encoded
            .Replace("-", "+")
            .Replace("_", "/");

        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        var bytes = Convert.FromBase64String(padded);
        return Encoding.UTF8.GetString(bytes);
    }

    public static T? DeepClone<T>(this T? instance) where T : class
    {
        if (instance == null) return null;
        var json = JsonSerializer.Serialize(instance);
        return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Clone failed");
    }
}
