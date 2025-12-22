using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace WS.WEB.Core
{
    public class CustomErrorBoundary : ErrorBoundary
    {
        [Parameter] public EventCallback<Exception> OnErrorCallback { get; set; }

        protected override async Task OnErrorAsync(Exception exception)
        {
            await base.OnErrorAsync(exception);

            if (OnErrorCallback.HasDelegate)
                await OnErrorCallback.InvokeAsync(exception);
        }
    }
}