using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DRMA.WEB.Core.Api
{
    public static class ApiHelper
    {
        public static async Task<string?> GetValueAsync(this HttpClient httpClient, string uri)
        {
            var response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent) return default;

                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new NotificationException(content);
            }
        }

        public static async Task<T?> GetJsonFromApi<T>(this HttpClient httpClient, string uri)
        {
            var response = await httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    if (response.StatusCode == HttpStatusCode.NoContent) return default;

                    return await response.Content.ReadFromJsonAsync<T>();
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
                var content = await response.Content.ReadAsStringAsync();
                throw new NotificationException(content);
            }
        }

        public static async Task<T?> GetJsonFromApi<T>(this HttpClient httpClient, HttpRequestMessage request)
        {
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    if (response.StatusCode == HttpStatusCode.NoContent) return default;

                    return await response.Content.ReadFromJsonAsync<T>();
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
                var content = await response.Content.ReadAsStringAsync();
                throw new NotificationException(content);
            }
        }
    }
}