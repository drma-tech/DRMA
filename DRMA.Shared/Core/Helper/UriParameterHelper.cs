using System.Text;

namespace DRMA.Shared.Core.Helper;

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
}