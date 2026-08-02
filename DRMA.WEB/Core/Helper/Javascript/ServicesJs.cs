using Microsoft.JSInterop;

namespace DRMA.WEB.Core.Helper.Javascript
{
    public class ServicesJs(IJSRuntime js) : JsModuleBase(js, "./js/services.js")
    {
        public Task InitGoogleAnalytics(string version, CancellationToken cancellationToken) => InvokeVoid("services.initGoogleAnalytics", cancellationToken, version);
    }
}