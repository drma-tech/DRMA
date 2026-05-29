using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization.Metadata;

namespace DRMA.WEB.Core.Api;

public enum ApiType
{
    Local,
    Anonymous,
}

public abstract class ApiCore(IHttpClientFactory factory, string? key, ApiType type)
{
    protected HttpClient LocalHttp => factory.CreateClient("Local");
    protected HttpClient AnonymousHttp => factory.CreateClient("Anonymous");

    private HttpClient GetHttp(ApiType type) => type switch
    {
        ApiType.Local => LocalHttp,
        ApiType.Anonymous => AnonymousHttp,
        _ => throw new NotImplementedException()
    };

    protected static Dictionary<string, int> CacheVersion { get; set; } = [];

    public static void ResetCacheVersion()
    {
        CacheVersion = [];
    }

    public static void SetNewVersion(string? key)
    {
        if (key.NotEmpty()) CacheVersion[key] = RandomNumberGenerator.GetInt32(1, 999999);
    }

    private Dictionary<string, string> GetVersion()
    {
        if (!CacheVersion.TryGetValue(key!, out _)) CacheVersion[key!] = RandomNumberGenerator.GetInt32(1, 999999);

        return new Dictionary<string, string> { { "v", CacheVersion[key!].ToString() } };
    }

    protected async Task<string?> GetStringAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            if (key.NotEmpty())
                return await GetHttp(type).GetStringAsync(uri.ConfigureParameters(GetVersion()), cancellationToken);
            return await GetHttp(type).GetStringAsync(uri, cancellationToken);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<byte[]> GetBytesAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            var http = GetHttp(type);

            if (key.NotEmpty())
                uri = uri.ConfigureParameters(GetVersion());

            return await http.GetByteArrayAsync(uri, cancellationToken);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<T?> GetAsync<T>(string uri, bool setNewVersion, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            if (setNewVersion) SetNewVersion(key);

            if (key.NotEmpty())
                return await GetHttp(type).GetJsonFromApi<T>(uri.ConfigureParameters(GetVersion()), cancellationToken);
            return await GetHttp(type).GetJsonFromApi<T>(uri, cancellationToken);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<HashSet<T>> GetListAsync<T>(string uri, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            if (key.NotEmpty())
            {
                var result = await GetHttp(type).GetJsonFromApi<HashSet<T>>(uri.ConfigureParameters(GetVersion()), cancellationToken);
                return result ?? [];
            }
            else
            {
                var result = await GetHttp(type).GetJsonFromApi<HashSet<T>>(uri, cancellationToken);
                return result ?? [];
            }
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<O?> PostAsync<I, O>(string uri, I? obj, JsonTypeInfo<I?> requestTypeInfo, JsonTypeInfo<O?> responseTypeInfo, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            SetNewVersion(key);

            var response = await GetHttp(type).PostAsJsonAsync(uri, obj, requestTypeInfo, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new NotificationException(content);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<O?> PutAsync<I, O>(string uri, I? obj, JsonTypeInfo<I?> requestTypeInfo, JsonTypeInfo<O?> responseTypeInfo, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            SetNewVersion(key);

            var response = await GetHttp(type).PutAsJsonAsync(uri, obj, requestTypeInfo, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync(responseTypeInfo, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new NotificationException(content);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }

    protected async Task<T?> DeleteAsync<T>(string uri, JsonTypeInfo<T?> typeInfo, CancellationToken cancellationToken)
    {
        try
        {
            await AppStateStatic.ProcessingStarted.PublishAsync();

            SetNewVersion(key);

            var response = await GetHttp(type).DeleteAsync(uri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent) return default;

            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new NotificationException(content);
        }
        finally
        {
            await AppStateStatic.ProcessingFinished.PublishAsync();
        }
    }
}
