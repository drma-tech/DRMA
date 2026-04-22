using System.Text;
using System.Text.Json;

namespace DRMA.Shared.Core.Helper;

public static class ParameterHelper
{
    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

    public static string ConvertFromStringToBase64(this string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }

    public static string ConvertFromBase64ToString(this string base64)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    public static string ConvertFromObjectToBase64<T>(this T obj) where T : class
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, IndentedOptions)));
    }

    public static byte[] ConvertFromObjectToBytes<T>(this T obj) where T : class
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, IndentedOptions));
    }

    public static byte[] ConvertFromBase64ToBytes(this string base64)
    {
        return Convert.FromBase64String(base64);
    }

    public static T? ConvertFromBase64ToObject<T>(this string base64) where T : class
    {
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
    }

    public static T? ConvertFromBytesToObject<T>(this byte[] bytes) where T : class
    {
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
    }

    public static string ConvertFromBytesToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
}