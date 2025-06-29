using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DRMA.WEB.Core.Api;

public abstract class ApiCore(IHttpClientFactory factory, string? key)
{
    protected HttpClient Http => factory.CreateClient("RetryHttpClient");
    protected static Dictionary<string, int> CacheVersion { get; set; } = [];

    public static void SetNewVersion(string? key)
    {
        if (key.NotEmpty()) CacheVersion[key!] = new Random().Next(1, 999999);
    }

    private Dictionary<string, string> GetVersion()
    {
        if (!CacheVersion.TryGetValue(key!, out _)) CacheVersion[key!] = new Random().Next(1, 999999);

        return new Dictionary<string, string> { { "v", CacheVersion[key!].ToString() } };
    }

    protected async Task<string?> GetValueAsync(string uri)
    {
        if (key.NotEmpty())
            return await Http.GetValueAsync(uri.ConfigureParameters(GetVersion()));
        return await Http.GetValueAsync(uri);
    }

    protected async Task<T?> GetAsync<T>(string uri, bool setNewVersion = false)
    {
        if (setNewVersion) SetNewVersion(key);

        if (key.NotEmpty())
            return await Http.GetJsonFromApi<T>(uri.ConfigureParameters(GetVersion()));
        return await Http.GetJsonFromApi<T>(uri);
    }

    protected async Task<HashSet<T>> GetListAsync<T>(string uri)
    {
        if (key.NotEmpty())
        {
            var result = await Http.GetJsonFromApi<HashSet<T>>(uri.ConfigureParameters(GetVersion()));
            return result ?? [];
        }
        else
        {
            var result = await Http.GetJsonFromApi<HashSet<T>>(uri);
            return result ?? [];
        }
    }

    protected async Task<T?> GetByRequest<T>(string uri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        request.Headers.TryAddWithoutValidation("content-type", "application/json;charset=utf-8");

        return await Http.GetJsonFromApi<T>(request);
    }

    protected async Task<O?> PostAsync<I, O>(string uri, I? obj)
    {
        SetNewVersion(key);

        var response = await Http.PostAsJsonAsync(uri, obj, new JsonSerializerOptions());

        if (response.StatusCode == HttpStatusCode.NoContent) return default;

        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<O>();

        var content = await response.Content.ReadAsStringAsync();
        throw new NotificationException(content);
    }

    protected async Task<O?> PutAsync<I, O>(string uri, I? obj)
    {
        SetNewVersion(key);

        var response = await Http.PutAsJsonAsync(uri, obj, new JsonSerializerOptions());

        if (response.StatusCode == HttpStatusCode.NoContent) return default;

        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<O>();

        var content = await response.Content.ReadAsStringAsync();
        throw new NotificationException(content);
    }

    protected async Task<T?> DeleteAsync<T>(string uri)
    {
        SetNewVersion(key);

        var response = await Http.DeleteAsync(uri);

        if (response.StatusCode == HttpStatusCode.NoContent) return default;

        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<T>();

        var content = await response.Content.ReadAsStringAsync();
        throw new NotificationException(content);
    }
}