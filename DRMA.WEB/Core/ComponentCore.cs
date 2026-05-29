using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Core;

/// <summary>
/// There is a memory cost when implementing this class. Use it when necessary.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ComponentCore<T> : ComponentBase, IDisposable where T : class
{
    [Inject] private ILogger<T> Logger { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected readonly CancellationTokenSource cts = new();
    protected ActionDispatcher<string> OnError { get; } = new();

    /// <summary>
    /// Mandatory data to fill out the page/component without delay (essential for bots, SEO, etc.)
    /// </summary>
    /// <returns></returns>
    protected virtual Task ProcessInitialData()
    {
        return Task.CompletedTask;
    }

    protected virtual Task ProcessComponentData()
    {
        return Task.CompletedTask;
    }

    protected virtual Task ProcessPopupData()
    {
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            AppStateStatic.BreakpointChanged.Subscribe(breakpoint => _ = InvokeAsync(StateHasChanged), cts.Token);

            await ProcessInitialData();
        }
        catch (Exception ex)
        {
            if (OnError != null)
                OnError.Publish(ex.Message);
            else
                await ProcessException(ex, false);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await ProcessComponentData();
                await ProcessPopupData();

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        catch (Exception ex)
        {
            if (OnError != null)
                OnError.Publish(ex.Message);
            else
                await ProcessException(ex);
        }
    }

    #region notification module

    protected async Task ShowInfo(string message)
    {
        if (!message.CanShowSnackbar()) return;

        Snackbar.Add(message, Severity.Info);

        await JsRuntime.Utils().PlayBeep(600, 120, "sine", CancellationToken.None);
        await JsRuntime.Utils().Vibrate([50], CancellationToken.None);
    }

    protected async Task ShowInfo(RenderFragment message)
    {
        Snackbar.Add(message, Severity.Info);

        await JsRuntime.Utils().PlayBeep(600, 120, "sine", CancellationToken.None);
        await JsRuntime.Utils().Vibrate([50], CancellationToken.None);
    }

    protected async Task ShowSuccess(string message)
    {
        if (!message.CanShowSnackbar()) return;

        Snackbar.Add(message, Severity.Success);

        await JsRuntime.Utils().PlayBeep(880, 100, "sine", CancellationToken.None);
        await JsRuntime.Utils().Vibrate([40], CancellationToken.None);
    }

    protected async Task ShowWarning(string message)
    {
        if (!message.CanShowSnackbar()) return;

        Snackbar.Add(message, Severity.Warning);

        await JsRuntime.Utils().PlayBeep(440, 200, "triangle", CancellationToken.None);
        await JsRuntime.Utils().Vibrate([100, 80, 100], CancellationToken.None);
    }

    protected async Task ShowError(string message)
    {
        if (!message.CanShowSnackbar()) return;

        Snackbar.Add(message, Severity.Error);

        await JsRuntime.Utils().PlayBeep(220, 400, "square", CancellationToken.None);
        await JsRuntime.Utils().Vibrate([200, 100, 200], CancellationToken.None);
    }

    protected async Task ProcessException(Exception ex, bool showMessage = true)
    {
        if (ex is NotificationException exc)
        {
            Logger.LogWarning(exc.Message);
            if (showMessage) await ShowWarning(exc.Message);
        }
        else if (ex is OperationCanceledException or TaskCanceledException or ObjectDisposedException)
        {
            //ignored
        }
        else
        {
            Logger.LogError(ex, ex.Message);
            if (showMessage) await ShowError(ex.Message);
        }
    }

    #endregion notification module

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
        GC.SuppressFinalize(this);
    }
}

public abstract class PageCore<T> : ComponentCore<T>, IBrowserViewportObserver, IAsyncDisposable where T : class
{
    [Inject] private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    [Parameter] public string? Culture { get; set; }

    /// <summary>
    /// NOTE: This method cannot depend on previously loaded variables, as events can be executed in parallel.
    /// </summary>
    /// <returns></returns>
    protected virtual Task ProcessPageData()
    {
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);

                await ProcessPageData();

                StateHasChanged();
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
