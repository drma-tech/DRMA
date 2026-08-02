using Microsoft.JSInterop;

namespace DRMA.WEB.Core.Helper.Javascript
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
}