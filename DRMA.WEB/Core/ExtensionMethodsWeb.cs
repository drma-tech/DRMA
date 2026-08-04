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

    public static IReadOnlyCollection<T> ToCollection<T>(this T? item) where T : struct
    {
        if (item == null) return [];
        return [item.Value];
    }

    public static string? GetRouteLanguage(string absolutePath)
    {
        var segments = absolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var lang = segments.FirstOrDefault()?.ToLowerInvariant();

        if (lang.IsValidLanguage())
        {
            return lang;
        }

        return null;
    }

    public static async Task<string> GetRouteLanguage(IJSRuntime js, string absolutePath)
    {
        var lang = GetRouteLanguage(absolutePath);

        if (lang.NotEmpty())
        {
            return lang;
        }

        return (await AppStateStatic.GetAppLanguage(js, CancellationToken.None)).ToString();
    }
}
