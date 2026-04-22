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

    public static string RemoveDiacritics(this string Text)
    {
        return DiacriticsRegex().Replace(Text.Normalize(NormalizationForm.FormD), string.Empty);
    }

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

    public static string? ToHash(this string? text)
    {
        if (text.Empty()) return null;

        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = MD5.HashData(bytes);

        return Convert.ToHexString(hash, 0, 8);
    }

    /// <summary>
    /// Removes invisible control characters that can break logs, JSON or storage.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
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
    /// Normalizes text so visually identical characters are stored the same way.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
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
}
