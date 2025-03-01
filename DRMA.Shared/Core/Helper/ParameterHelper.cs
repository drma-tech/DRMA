using Newtonsoft.Json;
using System.Text;

namespace DRMA.Shared.Core.Helper
{
    public static class ParameterHelper
    {
        public static string ConvertToString<T>(this T obj) where T : class
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
        }

        public static T? ConvertToObject<T>(this string text) where T : class
        {
            if (string.IsNullOrEmpty(text)) return null;

            var result = Encoding.UTF8.GetString(Convert.FromBase64String(text));
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}