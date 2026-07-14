using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DRMA.Shared.Core.Helper;

public static partial class StringHelper
{
    public static string Format(this string format, object? arg0, object? arg1 = null)
    {
        return string.Format(format, arg0, arg1);
    }

    public static string RemoveSpecialCharacters(this string str, char[]? customExceptions = null, char? replace = null)
    {
        return RemoveSpecialCharacters(str.AsSpan(), customExceptions, replace).ToString();
    }

    /// <summary>
    /// Removes characters that are not letters, digits, whitespace, or allowed exceptions. Optionally replaces removed characters with a specified replacement character.
    /// Useful for sanitizing input before storage or processing.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="customExceptions"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    /// <example>
    /// Input: "João@123!"
    /// Output: "Joao123"
    /// </example>
    public static ReadOnlySpan<char> RemoveSpecialCharacters(this ReadOnlySpan<char> str, char[]? customExceptions = null, char? replace = null)
    {
        Span<char> buffer = new char[str.Length];
        var idx = 0;
        char[] exceptions = ['-'];

        if (customExceptions != null) exceptions = exceptions.Union(customExceptions).ToArray();

        foreach (var c in str)
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || Array.Exists(exceptions, match => match == c))
            {
                buffer[idx] = c;
                idx++;
            }
            else if (replace != null)
            {
                buffer[idx] = replace.Value;
                idx++;
            }

        return buffer[..idx];
    }

    [GeneratedRegex(@"\p{Mn}", RegexOptions.Compiled)]
    private static partial Regex DiacriticsRegex();

    /// <summary>
    /// Removes diacritical marks (accents) from characters, converting them to their base ASCII equivalents.
    /// Useful for standardizing user input for comparison and identity matching.
    /// </summary>
    /// <param name="Text"></param>
    /// <returns></returns>
    /// <example>
    /// Input: "José"
    /// Output: "Jose"
    /// </example>
    public static string RemoveDiacritics(this string Text)
    {
        return DiacriticsRegex().Replace(Text.Normalize(NormalizationForm.FormD), string.Empty);
    }

    /// <summary>
    /// Converts a string into a URL-friendly slug by removing diacritics, invalid characters, and replacing whitespace with hyphens.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string? ToSlug(this string? str)
    {
        if (str == null) return null;

        str = str.ToLowerInvariant();
        str = str.RemoveDiacritics();
        str = str.RemoveSpecialCharacters();

        str = Regex.Replace(str, @"\s+", "-", RegexOptions.NonBacktracking); // Replace spaces with hyphens
        str = Regex.Replace(str, @"-+", "-", RegexOptions.NonBacktracking); // Replace multiple hyphens with a single one
        str = str.Trim('-'); // Trim leading and trailing hyphens

        return str;
    }

    /// <summary>
    /// Generates a deterministic SHA-256 based hash from a string input. The hash is used for identity matching, deduplication, and secure fingerprinting of normalized data.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string? ToHash(this string? text)
    {
        if (text.Empty()) return null;

        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Removes non-printable control characters that may break logs, JSON serialization, or storage systems.
    /// Keeps newline, carriage return, and tab characters.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <example>
    /// Input: "Hello\u0000World"
    /// Output: "HelloWorld"
    /// </example>
    public static string? RemoveUnsafeControlChars(this string? input)
    {
        if (input.Empty()) return null;

        var sb = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            if (!char.IsControl(ch) || ch == '\n' || ch == '\r' || ch == '\t')
                sb.Append(ch);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Normalizes a string to Unicode NFC (Normalization Form C), ensuring that visually identical characters are stored in a consistent binary representation.
    /// This is important for deterministic comparisons and hashing.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// /// <example>
    /// Input: "e\u0301" (é decomposed)
    /// Output: "é" (composed)
    /// </example>
    public static string? NormalizeToNfc(this string? input)
    {
        if (input.Empty()) return null;

        return input.Normalize(NormalizationForm.FormC);
    }

    private static readonly Regex UrlRegex = new(@"\b[a-z0-9-]{2,}\.(com|net|org|io|co|dev|app|me)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ObfuscatedRegex = new(@"\b/([a-z0-9- ]{2,}\s*)((?:\.|\[\.]|\(.\))|\[\s*dot\s*\]|\(\s*dot\s*\)|\s*dot\s*)\s*(com|net|org|io|co|dev|app|me)/gm\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ShortLinkRegex = new(@"(bit\.ly|tinyurl|goo\.gl|t\.co)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MentionRegex = new(@"@\w+", RegexOptions.Compiled);
    private static readonly Regex RepeatedCharSeqRegex = new(@"(.)\1{10,}", RegexOptions.Compiled);
    private static readonly Regex SymbolSeqRegex = new(@"[^\p{L}\p{N}\s]{10,}", RegexOptions.Compiled);
    private static readonly Regex EmojiRegex = new(@"\p{So}", RegexOptions.Compiled);

    /// <summary>
    /// Heuristically determines whether a text is likely to be spam based on patterns such as URLs, repeated characters, excessive symbols, mentions, or emoji spam.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsLikelySpam(string? text)
    {
        text = text.NormalizeToNfc();

        if (string.IsNullOrWhiteSpace(text)) return false;

        if (UrlRegex.IsMatch(text)) return true;
        if (ObfuscatedRegex.IsMatch(text)) return true;
        if (ShortLinkRegex.IsMatch(text)) return true;
        if (MentionRegex.IsMatch(text)) return true;

        var words = Regex.Split(text, @"\W+").Where(w => w.Length > 2).ToArray();
        if (words.GroupBy(w => w, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 4)) return true;

        if (RepeatedCharSeqRegex.IsMatch(text)) return true;
        if (SymbolSeqRegex.IsMatch(text)) return true;
        if (EmojiRegex.Matches(text).Count > 5) return true;

        if (text.Count(c => c == '\n') > 10) return true;

        return false;
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings. This measures how many single-character edits (insertions, deletions, substitutions) are required to transform one string into another.
    /// Useful for fuzzy matching and similarity detection, not for cryptographic or deterministic identity.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <example>
    /// Input: "JOSE" vs "JOSÉ"
    /// Output: 1
    /// </example>
    public static int Levenshtein(string s, string t)
    {
        var dp = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= t.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost
                );
            }
        }

        return dp[s.Length, t.Length];
    }

    /// <summary>
    /// Parses human-readable relative date expressions into an absolute UTC DateTime. Supports expressions like "yesterday" or "2 days ago".
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DateTime? ParseRelativeDate(this string? text)
    {
        if (text.Empty()) return null;

        text = text.ToLowerInvariant().Trim();

        if (text == "yesterday")
            return DateTime.UtcNow.AddDays(-1);

        var match = Regex.Match(text, @"(\d+)\s+(second|minute|hour|day|week|month|year)s?\s+ago");

        if (!match.Success)
            return null;

        int value = int.Parse(match.Groups[1].Value);
        string unit = match.Groups[2].Value;

        return unit switch
        {
            "second" => DateTime.UtcNow.AddSeconds(-value),
            "minute" => DateTime.UtcNow.AddMinutes(-value),
            "hour" => DateTime.UtcNow.AddHours(-value),
            "day" => DateTime.UtcNow.AddDays(-value),
            "week" => DateTime.UtcNow.AddDays(-(value * 7)),
            "month" => DateTime.UtcNow.AddMonths(-value),
            "year" => DateTime.UtcNow.AddYears(-value),
            _ => throw new InvalidOperationException("Invalid unit")
        };
    }
}