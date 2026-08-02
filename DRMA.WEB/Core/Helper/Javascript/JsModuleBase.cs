using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace DRMA.WEB.Core.Helper.Javascript
{
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
}