using Microsoft.JSInterop;
using System.Text.Json;
using DRMA.WEB.Shared;

namespace DRMA.WEB.Core.Helper
{
    public static class JsModuleLoader
    {
        private static readonly Dictionary<string, IJSObjectReference> cache = [];

        public static async Task<IJSObjectReference> Load(IJSRuntime js, string path)
        {
            if (!cache.TryGetValue(path, out var module))
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", path);
                cache[path] = module;
            }

            return module;
        }
    }

    public abstract class JsModuleBase(IJSRuntime js, string path)
    {
        protected async Task InvokeVoid(string method, params object?[] args)
        {
            var module = await JsModuleLoader.Load(js, path);
            await module.InvokeVoidAsync(method, args);
        }

        protected async Task<T> Invoke<T>(string method, params object?[] args)
        {
            var module = await JsModuleLoader.Load(js, path);
            return await module.InvokeAsync<T>(method, args);
        }
    }

    public static class JsModules
    {
        public static WindowJs Window(this IJSRuntime js) => new(js);

        public static UtilsJs Utils(this IJSRuntime js) => new(js);

        public static ServicesJs Services(this IJSRuntime js) => new(js);
    }

    public class WindowJs(IJSRuntime js)
    {
        public async Task HistoryBack() => await js.InvokeVoidAsync("history.back");

        public async Task InvokeVoidAsync(string identifier, params object?[]? args) => await js.InvokeVoidAsync(identifier, args);

        public async Task<T> InvokeAsync<T>(string identifier, params object?[]? args) => await js.InvokeAsync<T>(identifier, args);
    }

    public class UtilsJs(IJSRuntime js) : JsModuleBase(js, "./js/utils.js")
    {
        #region STORAGE

        public Task<string?> GetLocalStorage(string key) => Invoke<string?>("storage.getLocalStorage", key);

        public async Task<TValue?> GetLocalStorage<TValue>(string key)
        {
            var value = await Invoke<string?>("storage.getLocalStorage", key);
            return value != null ? JsonSerializer.Deserialize<TValue>(value) : default;
        }

        public Task SetLocalStorage(string key, string value) => InvokeVoid("storage.setLocalStorage", key, value);

        public Task SetLocalStorage(string key, object value) => InvokeVoid("storage.setLocalStorage", key, JsonSerializer.Serialize(value));

        public Task<string?> GetSessionStorage(string key) => Invoke<string?>("storage.getSessionStorage", key);

        public async Task<TValue?> GetSessionStorage<TValue>(string key)
        {
            var value = await Invoke<string?>("storage.getSessionStorage", key);
            return value != null ? JsonSerializer.Deserialize<TValue>(value) : default;
        }

        public Task SetSessionStorage(string key, string value) => InvokeVoid("storage.setSessionStorage", key, value);

        public Task SetSessionStorage(string key, object value) => InvokeVoid("storage.setSessionStorage", key, JsonSerializer.Serialize(value));

        public Task ShowCache() => InvokeVoid("storage.showCache");

        public Task ClearLocalStorage() => InvokeVoid("storage.clearLocalStorage");

        #endregion STORAGE

        #region NOTIFICATION

        public Task<string?> PlayBeep(int frequency, int duration, string type) => Invoke<string?>("notification.playBeep", frequency, duration, type);

        public Task<string?> Vibrate(int[] pattern) => Invoke<string?>("notification.vibrate", pattern);

        #endregion NOTIFICATION

        #region ENVIRONMENT

        public Task<string?> GetAppVersion() => Invoke<string?>("environment.getAppVersion");

        #endregion ENVIRONMENT

        #region INTEROP

        public Task DownloadFile(string filename, string contentType, object? content) => InvokeVoid("interop.downloadFile", filename, contentType, content);

        #endregion INTEROP
    }

    public class ServicesJs(IJSRuntime js) : JsModuleBase(js, "./js/services.js")
    {
        public Task InitGoogleAnalytics(string version) => InvokeVoid("services.initGoogleAnalytics", version);
    }
}
