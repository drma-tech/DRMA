using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace DRMA.WEB.Core;

public abstract class ComponentCore<T> : ComponentBase where T : class
{
    [Inject] private ILogger<T> Logger { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
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
    /// Non-critical data that may be delayed (popups, javascript handling, etc.)
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
        try
        {
            AppStateStatic.BreakpointChanged += client => StateHasChanged();
            AppStateStatic.BrowserWindowSizeChanged += client => StateHasChanged();
            await LoadEssentialDataAsync();
        }
        catch (Exception ex)
        {
            ex.ProcessException(Snackbar, Logger);
        }
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

    #region notification module

    protected async Task ShowInfo(string message)
    {
        Snackbar.Add(message, Severity.Info);

        await JsRuntime.Utils().PlayBeep(600, 120, "sine");
        await JsRuntime.Utils().Vibrate([50]);
    }

    protected async Task ShowInfo(RenderFragment message)
    {
        Snackbar.Add(message, Severity.Info);

        await JsRuntime.Utils().PlayBeep(600, 120, "sine");
        await JsRuntime.Utils().Vibrate([50]);
    }

    protected async Task ShowSuccess(string message)
    {
        Snackbar.Add(message, Severity.Success);

        await JsRuntime.Utils().PlayBeep(880, 100, "sine");
        await JsRuntime.Utils().Vibrate([40]);
    }

    protected async Task ShowWarning(string message)
    {
        Snackbar.Add(message, Severity.Warning);

        await JsRuntime.Utils().PlayBeep(440, 200, "triangle");
        await JsRuntime.Utils().Vibrate([100, 80, 100]);
    }

    protected async Task ShowError(string message)
    {
        Snackbar.Add(message, Severity.Error);

        await JsRuntime.Utils().PlayBeep(220, 400, "square");
        await JsRuntime.Utils().Vibrate([200, 100, 200]);
    }

    protected async Task ProcessException(Exception ex)
    {
        if (ex is NotificationException exc)
        {
            Logger.LogWarning(exc.Message);
            await ShowWarning(exc.Message);
        }
        else
        {
            Logger.LogError(ex, ex.Message);
            await ShowError(ex.Message);
        }
    }

    #endregion notification module
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
            await ProcessException(ex);
        }
    }

    #region BrowserViewportObserver

    Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

    Task IBrowserViewportObserver.NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
    {
        if (AppStateStatic.Breakpoint != browserViewportEventArgs.Breakpoint)
        {
            AppStateStatic.Size = GetSizeForBreakpoint(browserViewportEventArgs.Breakpoint);

            AppStateStatic.Breakpoint = browserViewportEventArgs.Breakpoint;
            AppStateStatic.BreakpointChanged?.Invoke(browserViewportEventArgs.Breakpoint);
        }

        if (AppStateStatic.BrowserWindowSize != browserViewportEventArgs.BrowserWindowSize)
        {
            AppStateStatic.BrowserWindowSize = browserViewportEventArgs.BrowserWindowSize;
            AppStateStatic.BrowserWindowSizeChanged?.Invoke(browserViewportEventArgs.BrowserWindowSize);
        }

        return InvokeAsync(StateHasChanged);
    }

    private static Size GetSizeForBreakpoint(Breakpoint breakpoint) => breakpoint switch
    {
        Breakpoint.Xs => Size.Small, //mobile view
        //Breakpoint.Sm => Size.Medium, //tablet view
        //Breakpoint.Md => Size.Medium,
        //Breakpoint.Lg => Size.Large,
        //Breakpoint.Xl => Size.Large,
        //Breakpoint.Xxl => Size.Large,
        _ => Size.Medium
    };

    public async ValueTask DisposeAsync()
    {
        await BrowserViewportService.UnsubscribeAsync(this);
        GC.SuppressFinalize(this);
    }

    #endregion BrowserViewportObserver
}