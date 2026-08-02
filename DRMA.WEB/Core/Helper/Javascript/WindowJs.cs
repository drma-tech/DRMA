using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace DRMA.WEB.Core.Helper.Javascript
{
    public class WindowJs(IJSRuntime js)
    {
        public async Task HistoryBack() => await js.InvokeVoidAsync("history.back");

        public async Task InvokeVoidAsync(string identifier, params object?[]? args) => await js.InvokeVoidAsync(identifier, args);

        public async Task<T> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>
            (string identifier, params object?[]? args) => await js.InvokeAsync<T>(identifier, args);
    }
}