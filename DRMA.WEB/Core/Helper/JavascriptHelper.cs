using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

        protected async Task<string?> Invoke(string identifier, params object?[]? args)
        {
            var module = await JsModuleLoader.Load(js, path);
            return await module.InvokeAsync<string?>(identifier, args);
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

        public async Task<string> InvokeAsync(string identifier, params object?[]? args) => await js.InvokeAsync<string>(identifier, args);
    }

    public class UtilsJs(IJSRuntime js) : JsModuleBase(js, "./js/utils.js")
    {
        #region STORAGE

        public enum BrowserStorageType
        {
            Local,
            Session
        }

        public async Task<T?> GetStorage<T>(string key, JsonTypeInfo<T?> typeInfo, BrowserStorageType storage = BrowserStorageType.Local)
        {
            var value = await Invoke(storage == BrowserStorageType.Local ? "storage.getLocalStorage" : "storage.getSessionStorage", key);
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (value.Empty())
            {
                return default;
            }
            else if (underlyingType == typeof(string))
            {
                return (T)(object)value;
            }
            else if (underlyingType.IsEnum)
            {
                var parsed = Enum.TryParse(underlyingType, value, ignoreCase: true, out var enumValue);
                var defined = parsed && enumValue != null && Enum.IsDefined(underlyingType, enumValue);
                return defined ? (T?)enumValue : default;
            }
            else
            {
                try
                {
                    return JsonSerializer.Deserialize(value, typeInfo);
                }
                catch (Exception)
                {
                    return default;
                }
            }
        }

        public Task SetStorage<T>(string key, T value, JsonTypeInfo<T> typeInfo, BrowserStorageType storage = BrowserStorageType.Local)
        {
            if (value is null) return RemoveStorage(storage, key);

            string storedValue;
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (value is string s)
            {
                storedValue = s.ToLowerInvariant();
            }
            else if (type.IsEnum || (underlyingType != null && underlyingType.IsEnum))
            {
                storedValue = value.ToString()?.ToLowerInvariant() ?? throw new UnhandledException("invalid enum value");
            }
            else
            {
                storedValue = JsonSerializer.Serialize(value, typeInfo);
            }

            return InvokeVoid(storage == BrowserStorageType.Local ? "storage.setLocalStorage" : "storage.setSessionStorage", key, storedValue);
        }

        public Task RemoveStorage(BrowserStorageType storage, string key)
        {
            return InvokeVoid(storage == BrowserStorageType.Local ? "storage.removeLocalStorage" : "storage.removeSessionStorage", key);
        }

        public Task ShowCache() => InvokeVoid("storage.showCache");

        public Task ClearAllStorage() => InvokeVoid("storage.clearAllStorage");

        #endregion STORAGE

        #region NOTIFICATION

        public Task<string?> PlayBeep(int frequency, int duration, string type) => Invoke("notification.playBeep", frequency, duration, type);

        public Task<string?> Vibrate(int[] pattern) => Invoke("notification.vibrate", pattern);

        #endregion NOTIFICATION

        #region ENVIRONMENT

        public Task<string?> GetAppVersion() => Invoke("environment.getAppVersion");

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