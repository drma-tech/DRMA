using System.Text;
using System.Text.Json;

namespace DRMA.Shared.Core.Helper;

public static class ParameterHelper
{
    public static string ConvertFromStringToBase64(this string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }

    public static string ConvertFromObjectToBase64<T>(this T obj) where T : class
    {
        return ConvertFromStringToBase64(JsonSerializer.Serialize(obj));
    }

    public static string ConvertFromBase64ToString(this string base64)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    public static T? ConvertFromBase64ToObject<T>(this string base64) where T : class
    {
        return JsonSerializer.Deserialize<T>(ConvertFromBase64ToString(base64));
    }
}
