using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace DRMA.WEB.Core.Javascript
{
    public class WindowJs(IJSRuntime js)
    {
        public async Task HistoryBack() => await js.InvokeVoidAsync("history.back");

        public async Task InvokeVoidAsync(string identifier, CancellationToken cancellationToken, params object?[]? args) => await js.InvokeVoidAsync(identifier, cancellationToken, args);

        public async Task<T> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>
            (string identifier, CancellationToken cancellationToken, params object?[]? args) => await js.InvokeAsync<T>(identifier, cancellationToken, args);
    }
}