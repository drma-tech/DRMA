using DRMA.Shared.Core.Helper;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace DRMA.WEB.Core.Api;

public enum ApiType
{
    Local,
    Anonymous,
    External
}

public abstract class ApiCore(IHttpClientFactory factory, string? key, ApiType type)
{
    protected HttpClient LocalHttp => factory.CreateClient("Local");
    protected HttpClient AnonymousHttp => factory.CreateClient("Anonymous");
    protected HttpClient ExternalHttp => factory.CreateClient("External");

    private HttpClient GetHttp(ApiType type) => type switch
    {
        ApiType.Local => LocalHttp,
        ApiType.Anonymous => AnonymousHttp,
        ApiType.External => ExternalHttp,
        _ => throw new NotImplementedException()
    };

    protected static Dictionary<string, int> CacheVersion { get; set; } = [];

    public static void ResetCacheVersion()
    {
        CacheVersion = [];
    }

    public static void SetNewVersion(string? key)
    {
        if (key.NotEmpty()) CacheVersion[key!] = RandomNumberGenerator.GetInt32(1, 999999);
    }

    private Dictionary<string, string> GetVersion()
    {
        if (!CacheVersion.TryGetValue(key!, out _)) CacheVersion[key!] = RandomNumberGenerator.GetInt32(1, 999999);

        return new Dictionary<string, string> { { "v", CacheVersion[key!].ToString() }, { "vs", AppStateStatic.Version ?? "" } };
    }

    private static Dictionary<string, string> GetSystemVersion()
    {
        return new Dictionary<string, string> { { "vs", AppStateStatic.Version ?? "" } };
    }

    protected async Task<string?> GetValueAsync(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            if (key.NotEmpty())
                return await GetHttp(type).GetValueAsync(uri.ConfigureParameters(GetVersion()), cancellationToken);
            return await GetHttp(type).GetValueAsync(uri.ConfigureParameters(GetSystemVersion()), cancellationToken);
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }

    protected async Task<T?> GetAsync<T>(string uri, bool setNewVersion = false, CancellationToken cancellationToken = default)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            if (setNewVersion) SetNewVersion(key);

            if (key.NotEmpty())
                return await GetHttp(type).GetJsonFromApi<T>(uri.ConfigureParameters(GetVersion()), cancellationToken);
            return await GetHttp(type).GetJsonFromApi<T>(uri.ConfigureParameters(GetSystemVersion()), cancellationToken);
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }

    protected async Task<HashSet<T>> GetListAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            if (key.NotEmpty())
            {
                var result = await GetHttp(type).GetJsonFromApi<HashSet<T>>(uri.ConfigureParameters(GetVersion()), cancellationToken);
                return result ?? [];
            }
            else
            {
                var result = await GetHttp(type).GetJsonFromApi<HashSet<T>>(uri.ConfigureParameters(GetSystemVersion()), cancellationToken);
                return result ?? [];
            }
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }

    protected async Task<O?> PostAsync<I, O>(string uri, I? obj)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            SetNewVersion(key);

            var response = await GetHttp(type).PostAsJsonAsync(uri, obj, new JsonSerializerOptions());

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<O>();

            var content = await response.Content.ReadAsStringAsync();
            throw new NotificationException(content);
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }

    protected async Task<O?> PutAsync<I, O>(string uri, I? obj)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            SetNewVersion(key);

            var response = await GetHttp(type).PutAsJsonAsync(uri, obj, new JsonSerializerOptions());

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<O>();

            var content = await response.Content.ReadAsStringAsync();
            throw new NotificationException(content);
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }

    protected async Task<T?> DeleteAsync<T>(string uri)
    {
        try
        {
            AppStateStatic.ProcessingStarted?.Invoke();

            SetNewVersion(key);

            var response = await GetHttp(type).DeleteAsync(uri);

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<T>();

            var content = await response.Content.ReadAsStringAsync();
            throw new NotificationException(content);
        }
        finally
        {
            AppStateStatic.ProcessingFinished?.Invoke();
        }
    }
}