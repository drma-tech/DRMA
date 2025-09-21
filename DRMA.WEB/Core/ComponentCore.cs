using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace DRMA.WEB.Core;

public abstract class ComponentCore<T> : ComponentBase where T : class
{
    [Inject] protected ILogger<T> Logger { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected static Breakpoint Breakpoint => AppStateStatic.Breakpoint;
    protected static BrowserWindowSize? BrowserWindowSize => AppStateStatic.BrowserWindowSize;

    /// <summary>
    /// Mandatory data to fill out the page/component without delay (essential for bots, SEO, etc.)
    /// </summary>
    /// <returns></returns>
    protected virtual Task LoadEssentialDataAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Non-critical data that may be delayed (popups, javascript handling, authenticated user data, etc.)
    ///
    /// NOTE: This method cannot depend on previously loaded variables, as events can be executed in parallel.
    /// </summary>
    /// <returns></returns>
    protected virtual Task LoadNonEssentialDataAsync()
    {
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        AppStateStatic.BreakpointChanged += client => StateHasChanged();
        AppStateStatic.BrowserWindowSizeChanged += client => StateHasChanged();
        await LoadEssentialDataAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await LoadNonEssentialDataAsync();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ex.ProcessException(Snackbar, Logger);
        }
    }
}

public abstract class PageCore<T> : ComponentCore<T>, IBrowserViewportObserver, IAsyncDisposable where T : class
{
    [Inject] private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        catch (Exception ex)
        {
            ex.ProcessException(Snackbar, Logger);
        }
    }

    #region BrowserViewportObserver

    Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

    Task IBrowserViewportObserver.NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
    {
        AppStateStatic.Breakpoint = browserViewportEventArgs.Breakpoint;
        AppStateStatic.BreakpointChanged?.Invoke(browserViewportEventArgs.Breakpoint);

        AppStateStatic.BrowserWindowSize = browserViewportEventArgs.BrowserWindowSize;
        AppStateStatic.BrowserWindowSizeChanged?.Invoke(browserViewportEventArgs.BrowserWindowSize);

        return InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        await BrowserViewportService.UnsubscribeAsync(this);
        GC.SuppressFinalize(this);
    }

    #endregion BrowserViewportObserver
}