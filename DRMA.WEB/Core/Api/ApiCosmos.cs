using DRMA.WEB.Shared;

namespace DRMA.WEB.Core.Api;

public abstract class ApiCosmos<T>(IHttpClientFactory factory, string? key) : ApiCore(factory, key) where T : class
{
    public Action<T?>? DataChanged { get; set; }

    private string BaseEndpoint => Http.BaseAddress?.ToString().Contains("localhost") ?? true
        ? "http://localhost:7253/api/"
        : $"{Http.BaseAddress}api/";

    protected async Task<string?> GetValueAsync(string endpoint, RenderControlCore<string?>? core)
    {
        core?.LoadingStarted?.Invoke();

        string? result = null;

        try
        {
            result = await GetValueAsync($"{BaseEndpoint}{endpoint}");
            return result;
        }
        finally
        {
            core?.LoadingFinished?.Invoke(result);
        }
    }

    protected async Task<T?> GetAsync(string endpoint, RenderControlCore<T?>? core, bool setNewVersion = false)
    {
        core?.LoadingStarted?.Invoke();

        T? obj = null;

        try
        {
            obj = await GetAsync<T>($"{BaseEndpoint}{endpoint}", setNewVersion);
            return obj;
        }
        finally
        {
            core?.LoadingFinished?.Invoke(obj);
        }
    }

    protected async Task<HashSet<T>> GetListAsync(string endpoint, RenderControlCore<HashSet<T>>? core)
    {
        core?.LoadingStarted?.Invoke();

        HashSet<T> list = [];

        try
        {
            list = await GetListAsync<T>($"{BaseEndpoint}{endpoint}");
            return list;
        }
        finally
        {
            core?.LoadingFinished?.Invoke(list);
        }
    }

    protected async Task<T?> PostAsync(string endpoint, RenderControlCore<T?>? core, T? obj)
    {
        return await PostAsync<T>(endpoint, core, obj);
    }

    protected async Task<T?> PostAsync<I>(string endpoint, RenderControlCore<T?>? core, I? obj) where I : class
    {
        T? result = null;

        try
        {
            core?.ProcessingStarted?.Invoke();

            result = await PostAsync<I, T>($"{BaseEndpoint}{endpoint}", obj);

            DataChanged?.Invoke(result);

            return result;
        }
        finally
        {
            core?.ProcessingFinished?.Invoke(result);
        }
    }

    protected async Task<T?> PutAsync<I>(string endpoint, RenderControlCore<T?>? core, I? obj) where I : class
    {
        T? result = null;

        try
        {
            core?.ProcessingStarted?.Invoke();

            result = await PutAsync<I, T>($"{BaseEndpoint}{endpoint}", obj);

            DataChanged?.Invoke(result);

            return result;
        }
        finally
        {
            core?.ProcessingFinished?.Invoke(result);
        }
    }

    protected async Task<T?> DeleteAsync(string endpoint, RenderControlCore<T?>? core)
    {
        T? result = null;

        try
        {
            core?.ProcessingStarted?.Invoke();

            result = await DeleteAsync<T>($"{BaseEndpoint}{endpoint}");

            DataChanged?.Invoke(result);

            return result;
        }
        finally
        {
            core?.ProcessingFinished?.Invoke(result);
        }
    }
}