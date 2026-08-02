using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DRMA.WEB.Core.Component
{
    public abstract class PageParameterCore<T> : ComponentParameterCore<T>, IBrowserViewportObserver, IAsyncDisposable where T : class
    {
        [Inject] private IBrowserViewportService BrowserViewportService { get; set; } = null!;

        [Parameter] public string? Culture { get; set; }

        protected override bool ShowExceptions => true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);
            }
        }

        #region BrowserViewportObserver

        Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

        Task IBrowserViewportObserver.NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
        {
            if (AppStateStatic.Breakpoint != browserViewportEventArgs.Breakpoint)
            {
                AppStateStatic.Size = browserViewportEventArgs.Breakpoint == Breakpoint.Xs ? Size.Small : Size.Medium;
                AppStateStatic.Breakpoint = browserViewportEventArgs.Breakpoint;
                AppStateStatic.BreakpointChanged.Publish(browserViewportEventArgs.Breakpoint);
            }

            return InvokeAsync(StateHasChanged);
        }

        public virtual async ValueTask DisposeAsync()
        {
            Dispose();
            await BrowserViewportService.UnsubscribeAsync(this);
            GC.SuppressFinalize(this);
        }

        #endregion BrowserViewportObserver
    }
}