using DRMA.WEB.Core.Helper.Javascript;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Core.Component;

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

    protected CancellationTokenSource Cts { get; } = new();
    protected virtual bool ShowExceptions => false;

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

            AppStateStatic.BreakpointChanged.Subscribe((bp) => _ = InvokeAsync(StateHasChanged), Cts.Token);

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
            Logger.Warning(exc.Message);
            if (showMessage) await ShowWarning(exc.Message);
        }
        else if (ex is OperationCanceledException or TaskCanceledException or ObjectDisposedException)
        {
            //ignored
        }
        else
        {
            Logger.Error(ex, ex.Message);
            if (showMessage) await ShowError(ex.Message);
        }
    }

    #endregion notification module

    #region Dispose

    private bool isDisposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            Cts.Cancel();
            Cts.Dispose();
        }

        isDisposed = true;
    }

    #endregion Dispose
}