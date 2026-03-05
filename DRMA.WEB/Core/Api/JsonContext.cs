using System.Text.Json.Serialization;

namespace DRMA.WEB.Core.Api
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(Platform?))]
    [JsonSerializable(typeof(bool?))]
    [JsonSerializable(typeof(string))]
    internal partial class JavascriptContext : JsonSerializerContext
    {
    }
}