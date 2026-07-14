using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Core;

/// <summary>
/// There is a memory cost when implementing this class. Use it when necessary.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseComponentCore<T> : ComponentBase, IDisposable where T : class
{
    [Inject] private ILogger<T> Logger { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected readonly CancellationTokenSource cts = new();
    protected virtual bool ShowExceptions => false;

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

    #region Dispose

    private bool isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            cts.Cancel();
            cts.Dispose();
        }

        isDisposed = true;
    }

    #endregion Dispose
}

/// <summary>
/// There is a memory cost when implementing this class. Use it when necessary.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ComponentCore<T> : BaseComponentCore<T> where T : class
{
    protected readonly TaskHelper _taskHelper = new();
    protected override bool ShowExceptions => false;

    /// <summary>
    /// To load static data that does not change and does not depend on parameters.
    /// </summary>
    /// <returns></returns>
    protected virtual Task LoadStaticDataAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// To process data that depends on the DOM, JavaScript, or element references.
    ///
    /// Note: Returns true when the component state changed and a re-render is required.
    /// </summary>
    /// <returns></returns>
    protected virtual Task<bool> LoadInteropDataAsync(Microsoft.JSInterop.IJSRuntime JsRuntime)
    {
        return Task.FromResult(false);
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await base.OnInitializedAsync();

            AppStateStatic.BreakpointChanged.Subscribe((bp) => _ = InvokeAsync(StateHasChanged), cts.Token);

            await LoadStaticDataAsync();
        }
        catch (Exception ex)
        {
            await ProcessException(ex, ShowExceptions);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender && await LoadInteropDataAsync(JsRuntime))
            {
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            await ProcessException(ex, ShowExceptions);
        }
    }
}

public abstract class ComponentParameterCore<T> : ComponentCore<T> where T : class
{
    /// <summary>
    /// To load temporary data that may change and depends on parameters.
    /// </summary>
    /// <returns></returns>
    protected abstract Task LoadParameterDataAsync();

    private object? _lastParameterKey;

    protected abstract object? GetParameterKey();

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            await base.OnParametersSetAsync();

            var parameterKey = GetParameterKey();

            if (!Equals(_lastParameterKey, parameterKey))
            {
                _lastParameterKey = parameterKey;

                await LoadParameterDataAsync();
            }
        }
        catch (Exception ex)
        {
            await ProcessException(ex, ShowExceptions);
        }
    }
}

public abstract class PageCore<T> : ComponentCore<T>, IBrowserViewportObserver, IAsyncDisposable where T : class
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