using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DRMA.WEB.Core.Helper
{
    public static class JsModuleLoader
    {
        private static readonly Dictionary<string, IJSObjectReference> cache = [];

        public static async Task<IJSObjectReference> Load(IJSRuntime js, string path, CancellationToken cancellationToken)
        {
            if (!cache.TryGetValue(path, out var module))
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, path);
                cache[path] = module;
            }

            return module;
        }
    }

    public abstract class JsModuleBase(IJSRuntime js, string path)
    {
        protected async Task InvokeVoid(string identifier, CancellationToken cancellationToken, params object?[] args)
        {
            var module = await JsModuleLoader.Load(js, path, cancellationToken);
            await module.InvokeVoidAsync(identifier, cancellationToken, args);
        }

        protected async Task<T> Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>
            (string identifier, CancellationToken cancellationToken, params object?[] args)
        {
            var module = await JsModuleLoader.Load(js, path, cancellationToken);
            return await module.InvokeAsync<T>(identifier, cancellationToken, args);
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

        public async Task<T> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>
            (string identifier, params object?[]? args) => await js.InvokeAsync<T>(identifier, args);
    }

    public class UtilsJs(IJSRuntime js) : JsModuleBase(js, "./js/utils.js")
    {
        #region STORAGE

        public enum BrowserStorageType
        {
            Local,
            Session
        }

        public async Task<T?> GetStorage<T>(string key, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken, BrowserStorageType storage = BrowserStorageType.Local)
        {
            var value = await Invoke<string?>(storage == BrowserStorageType.Local ? "storage.getLocalStorage" : "storage.getSessionStorage", cancellationToken, key);
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
                var parsed = System.Enum.TryParse(underlyingType, value, ignoreCase: true, out var enumValue);
                var defined = parsed && enumValue != null && System.Enum.IsDefined(underlyingType, enumValue);
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

        public Task SetStorage<T>(string key, T value, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken, BrowserStorageType storage = BrowserStorageType.Local)
        {
            if (value is null) return RemoveStorage(storage, key, cancellationToken);

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

            return InvokeVoid(storage == BrowserStorageType.Local ? "storage.setLocalStorage" : "storage.setSessionStorage", cancellationToken, key, storedValue);
        }

        public Task RemoveStorage(BrowserStorageType storage, string key, CancellationToken cancellationToken)
        {
            return InvokeVoid(storage == BrowserStorageType.Local ? "storage.removeLocalStorage" : "storage.removeSessionStorage", cancellationToken, key);
        }

        public Task ShowCache(CancellationToken cancellationToken) => InvokeVoid("storage.showCache", cancellationToken);

        public Task ClearAllStorage() => InvokeVoid("storage.clearAllStorage", CancellationToken.None);

        #endregion STORAGE

        #region NOTIFICATION

        public Task<string?> PlayBeep(int frequency, int duration, string type, CancellationToken cancellationToken) => Invoke<string?>("notification.playBeep", cancellationToken, frequency, duration, type);

        public Task<string?> Vibrate(int[] pattern, CancellationToken cancellationToken) => Invoke<string?>("notification.vibrate", cancellationToken, pattern);

        #endregion NOTIFICATION

        #region ENVIRONMENT

        public Task<string?> GetAppVersion(CancellationToken cancellationToken) => Invoke<string?>("environment.getAppVersion", cancellationToken);

        public Task<string?> GetBrowserName(CancellationToken cancellationToken) => Invoke<string?>("environment.getBrowserName", cancellationToken);

        public Task<string?> GetBrowserVersion(CancellationToken cancellationToken) => Invoke<string?>("environment.getBrowserVersion", cancellationToken);

        public Task<string?> GetOperatingSystem(CancellationToken cancellationToken) => Invoke<string?>("environment.getOperatingSystem", cancellationToken);

        public Task<bool> IsAdBlocked(CancellationToken cancellationToken) => Invoke<bool>("environment.isAdBlocked", cancellationToken);

        #endregion ENVIRONMENT

        #region INTEROP

        public Task DownloadFile(string filename, string contentType, object? content, CancellationToken cancellationToken) => InvokeVoid("interop.downloadFile", cancellationToken, filename, contentType, content);

        public Task Share(string? title, string? text, string? url, CancellationToken cancellationToken) => InvokeVoid("interop.share", cancellationToken, title, text, url);

        #endregion INTEROP
    }

    public class ServicesJs(IJSRuntime js) : JsModuleBase(js, "./js/services.js")
    {
        public Task InitGoogleAnalytics(string version, CancellationToken cancellationToken) => InvokeVoid("services.initGoogleAnalytics", cancellationToken, version);
    }
}