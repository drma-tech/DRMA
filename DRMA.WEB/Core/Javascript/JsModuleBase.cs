using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace DRMA.WEB.Core.Javascript
{
    public abstract class JsModuleBase(IJSRuntime js, string path)
    {
        protected async Task InvokeVoid(string identifier, CancellationToken cancellationToken, params object?[] args)
        {
            try
            {
                var module = await JsModuleLoader.Load(js, path, cancellationToken);
                await module.InvokeVoidAsync(identifier, cancellationToken, args);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex) when (ex.Message.Contains("AbortError", StringComparison.OrdinalIgnoreCase))
            {
                // ignore
            }
        }

        protected async Task<T> Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>
            (string identifier, CancellationToken cancellationToken, params object?[] args)
        {
            try
            {
                var module = await JsModuleLoader.Load(js, path, cancellationToken);
                return await module.InvokeAsync<T>(identifier, cancellationToken, args);
            }
            catch (OperationCanceledException)
            {
                return default!;
            }
            catch (Exception ex) when (ex.Message.Contains("AbortError", StringComparison.OrdinalIgnoreCase))
            {
                return default!;
            }
        }
    }
}