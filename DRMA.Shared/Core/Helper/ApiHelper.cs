using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DRMA.Shared.Core.Helper;

public static class ApiHelper
{
    public static async Task<string?> GetValueAsync(this HttpClient httpClient, string uri, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(uri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NoContent) return null;

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (content.Empty()) content = response.ReasonPhrase ?? "Unknown error";
        throw new UnhandledException(content);
    }

    public static async Task<T?> GetJsonFromApi<T>(this HttpClient httpClient, string uri, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(uri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.NoContent) return default;

                return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions(), cancellationToken);
            }
            catch (NotSupportedException ex) // When content type is not valid
            {
                throw new InvalidDataException("The content type is not supported", ex.InnerException ?? ex);
            }
            catch (JsonException ex) // Invalid JSON
            {
                throw new InvalidDataException("invalid json", ex.InnerException ?? ex);
            }
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Empty()) content = response.ReasonPhrase ?? "Unknown error";
            throw new UnhandledException(content);
        }
    }
}
