using Microsoft.JSInterop;
using System.Text.Json;

namespace DRMA.WEB.Core.Helper
{
    public static class JavascriptHelper
    {
        public static async Task<string?> GetLocalStorage(this IJSRuntime js, string key)
        {
            return await js.JavascriptAsync<string?>("GetLocalStorage", key);
        }

        public static async Task<TValue?> GetLocalStorage<TValue>(this IJSRuntime js, string key)
        {
            var value = await js.JavascriptAsync<string?>("GetLocalStorage", key);
            return value != null ? JsonSerializer.Deserialize<TValue>(value) : default;
        }

        public static async Task<string?> GetSessionStorage(this IJSRuntime js, string key)
        {
            return await js.JavascriptAsync<string?>("GetSessionStorage", key);
        }

        public static async Task<TValue?> GetSessionStorage<TValue>(this IJSRuntime js, string key)
        {
            var value = await js.JavascriptAsync<string?>("GetSessionStorage", key);
            return value != null ? JsonSerializer.Deserialize<TValue>(value) : default;
        }

        public static async Task<TValue?> JavascriptAsync<TValue>(this IJSRuntime js, string method, params object?[]? args)
        {
            try
            {
                return await js.InvokeAsync<TValue>(method, args);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static async Task SetLocalStorage(this IJSRuntime js, string key, string value)
        {
            await js.JavascriptVoidAsync("SetLocalStorage", key, value);
        }

        public static async Task SetLocalStorage(this IJSRuntime js, string key, object value)
        {
            await js.JavascriptVoidAsync("SetLocalStorage", key, JsonSerializer.Serialize(value));
        }

        public static async Task SetSessionStorage(this IJSRuntime js, string key, string value)
        {
            await js.JavascriptVoidAsync("SetSessionStorage", key, value);
        }

        public static async Task SetSessionStorage(this IJSRuntime js, string key, object value)
        {
            await js.JavascriptVoidAsync("SetSessionStorage", key, JsonSerializer.Serialize(value));
        }

        public static async Task JavascriptVoidAsync(this IJSRuntime js, string method, params object?[]? args)
        {
            await js.InvokeVoidAsync(method, args);
        }
    }
}
