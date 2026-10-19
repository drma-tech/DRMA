using System.Globalization;

namespace DRMA.WEB.Core.Extensions;

public static class StringFormatExtensions
{
    public static string CustomFormat(this string format, object? arg0, object? arg1 = null)
    {
        return string.Format(CultureInfo.DefaultThreadCurrentCulture, format, arg0, arg1);
    }
}
