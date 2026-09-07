using Microsoft.JSInterop;
using MudBlazor;

namespace DRMA.WEB.Layout
{
    public partial class MainLayout : IDisposable
    {
        private MudThemeProvider? _mudThemeProvider;
        private bool _darkMode;
        protected CancellationTokenSource Cts { get; } = new();

        protected override void OnInitialized()
        {
            try
            {
                // *************************************
                // attention: avoid using asynchronous calls here, as it may affect static html generation (especially for anonymous users)
                // *************************************

                BufferedEvent.Register(nameof(ShowError), async (string msg) => { await ShowNotificationError(msg); });

                AppStateStatic.DarkModeChanged += dark => { _darkMode = dark; StateHasChanged(); };
                AppStateStatic.BreakpointChanged.Subscribe(breakpoint => StateHasChanged(), Cts.Token);
            }
            catch (Exception ex)
            {
                ex.ProcessException(Snackbar, Logger);
            }
        }

        /// <summary>
        /// Do not process anything here related to authenticated users. (use UserStateChanged instead)
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                //Get the value at the beginning (NotifyBrowserViewportChangeAsync is too slow)
                AppStateStatic.Breakpoint = await BrowserViewportService.GetCurrentBreakpointAsync();
                AppStateStatic.Size = AppStateStatic.Breakpoint == Breakpoint.Xs ? Size.Small : Size.Medium;
                AppStateStatic.BreakpointChanged.Publish(AppStateStatic.Breakpoint);

                try
                {
                    await ApplyDarkMode(Cts.Token);
                }
                catch (Exception ex)
                {
                    ex.ProcessException(Snackbar, Logger);
                }
            }
        }

        private async Task ApplyDarkMode(CancellationToken cancellationToken)
        {
            var darkMode = await AppStateStatic.GetDarkMode(JsRuntime, Cts.Token);

            if (darkMode == null && _mudThemeProvider != null)
            {
                var system = await _mudThemeProvider.GetSystemDarkModeAsync();
                darkMode = system;

                await JsRuntime.Utils().SetStorage("dark-mode", darkMode ?? false, JavascriptContext.Default.Boolean, cancellationToken);
            }

            AppStateStatic.ChangeDarkMode(darkMode ?? false);
        }

        protected async Task ShowNotificationError(string message)
        {
            if (!message.CanShowSnackbar()) return;

            Snackbar.Add(message, Severity.Error);

            await JsRuntime.Utils().PlayBeep(220, 400, "square", CancellationToken.None);
            await JsRuntime.Utils().Vibrate([200, 100, 200], CancellationToken.None);
        }

        [JSInvokable]
        public static void ShowError(string error)
        {
            _ = BufferedEvent.Invoke(nameof(ShowError), error);
        }

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
    }
}