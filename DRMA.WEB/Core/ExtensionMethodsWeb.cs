using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Specialized;
using System.Web;

namespace DRMA.WEB.Core;

public static class ExtensionMethodsWeb
{
    public static NameValueCollection QueryString(this NavigationManager navigationManager)
    {
        return HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);
    }

    public static string? QueryString(this NavigationManager navigationManager, string key)
    {
        return navigationManager.QueryString()[key];
    }

    public static HashSet<T> ToHashSet<T>(this T? item) where T : struct
    {
        if (item == null) return [];
        return [item.Value];
    }

    public static string? GetRouteLanguage(Uri uri)
    {
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var lang = segments.FirstOrDefault()?.ToLowerInvariant();

        if (lang.IsValidLanguage())
        {
            return lang!;
        }
        else
        {
            return null;
        }
    }

    public static async Task<string> GetRouteLanguage(IJSRuntime js, Uri uri)
    {
        var lang = GetRouteLanguage(uri);

        if (lang.NotEmpty())
        {
            return lang;
        }
        else
        {
            return (await AppStateStatic.GetAppLanguage(js)).ToString();
        }
    }
}
