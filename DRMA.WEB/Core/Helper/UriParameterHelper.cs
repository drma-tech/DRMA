using System.Text;

namespace DRMA.WEB.Core.Helper;

public static class UriParameterHelper
{
    public static string ConfigureParameters(this string uri, Dictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0) return uri;

        var sb = new StringBuilder(uri);
        var started = uri.Contains('?');

        for (var i = 0; i < parameters.Count; i++)
        {
            var item = parameters.ElementAt(i);

            if (i == 0)
                sb.Append($"{(started ? "&" : "?")}{item.Key}={item.Value}");
            else
                sb.Append($"&{item.Key}={item.Value}");
        }

        return sb.ToString();
    }

    public static string? GetParameter(this string uri, string key)
    {
        if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(key)) return null;
        var queryStart = uri.IndexOf('?');
        if (queryStart == -1 || queryStart == uri.Length - 1) return null;
        var query = uri[(queryStart + 1)..];
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == key)
            {
                return parts[1];
            }
        }
        return null;
    }
}