using System.Globalization;

namespace DRMA.Shared.Core.Helper;

public static class AttributeHelper
{
    public static DateTime? GetDate(this string? value)
    {
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out _))
            return DateTime.Parse(value, CultureInfo.CurrentCulture);
        return null;
    }

    public static string FormatRuntime(this int? runtime)
    {
        if (!runtime.HasValue || runtime == 0) return "";
        var time = TimeSpan.FromMinutes(runtime.Value);
        return $"{time.Hours}h {time.Minutes}m";
    }
}