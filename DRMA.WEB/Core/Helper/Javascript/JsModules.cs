using Microsoft.JSInterop;

namespace DRMA.WEB.Core.Helper.Javascript
{
    public static class JsModules
    {
        public static WindowJs Window(this IJSRuntime js) => new(js);

        public static UtilsJs Utils(this IJSRuntime js) => new(js);

        public static ServicesJs Services(this IJSRuntime js) => new(js);
    }
}