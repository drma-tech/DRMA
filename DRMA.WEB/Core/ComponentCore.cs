using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Core;

public abstract class ComponentCore<T> : ComponentBase where T : class
{
    [Inject] private ILogger<T> Logger { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected Action<string>? OnError { get; set; }

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
            AppStateStatic.BreakpointChanged += breakpoint => StateHasChanged();
            AppStateStatic.BrowserWindowSizeChanged += size => StateHasChanged();

            await ProcessInitialData();
        }
        catch (Exception ex)
        {
            if (OnError != null)
                OnError(ex.Message);
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
                OnError(ex.Message);
            else
                await ProcessException(ex);
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

    protected async Task ProcessException(Exception ex, bool showMessage = true)
    {
        if (ex is NotificationException exc)
        {
            Logger.LogWarning(exc.Message);
            if (showMessage) await ShowWarning(exc.Message);
        }
        else
        {
            Logger.LogError(ex, ex.Message);
            if (showMessage) await ShowError(ex.Message);
        }
    }

    #endregion notification module

    public async Task OpenExternalWebsite(string? url)
    {
        if (url.Empty()) return;

        var encodedUrl = System.Net.WebUtility.HtmlEncode(url);

        MarkupString message = new($@"
        <div style='text-align:left; line-height:1.5; font-size:14px;'>

            <p style='margin-bottom:12px;'>
                You are about to open an external website:
            </p>

            <div style='background-color:#f5f5f5; padding:10px; border-radius:6px; word-break:break-word; color: black; font-family:monospace; font-size:13px; margin-bottom:16px;'>
                {encodedUrl}
            </div>

            <p style='margin-bottom:10px; text-align: justify;'>
                This website is operated by a third party and is not owned, controlled, or affiliated with DRMA.
            </p>

            <p style='margin-bottom:10px; text-align: justify;'>
                We are not responsible for the content, policies, or practices of this external site.
            </p>

            <p style='font-weight:500; text-align: justify;'>
                By continuing, you acknowledge that you are leaving the app environment and accessing an independent website.
            </p>

        </div>");

        bool? result = await DialogService.ShowMessageBoxAsync("External Website Notice", message, yesText: "Continue to Website", cancelText: "Cancel");

        if (result == true)
        {
            await JsRuntime.Window().InvokeVoidAsync("open", url, "_blank");
        }
    }
}

public abstract class PageCore<T> : ComponentCore<T>, IBrowserViewportObserver, IAsyncDisposable where T : class
{
    [Inject] private IBrowserViewportService BrowserViewportService { get; set; } = null!;

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

    public virtual async ValueTask DisposeAsync()
    {
        await BrowserViewportService.UnsubscribeAsync(this);
        GC.SuppressFinalize(this);
    }

    #endregion BrowserViewportObserver
}
