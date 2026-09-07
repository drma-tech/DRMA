using MudBlazor;

namespace DRMA.WEB.Layout
{
    public partial class HeadLayout : IDisposable
    {
        private string Culture => Navigation.GetCulture();

        private bool _openMenu;

        private int _processingCount;
        private bool _processing => _processingCount > 0;

        protected CancellationTokenSource Cts { get; } = new();

        protected override void OnInitialized()
        {
            Navigation.LocationChanged += delegate { StateHasChanged(); };

            AppStateStatic.BreakpointChanged.Subscribe(breakpoint => StateHasChanged(), Cts.Token);

            //avoid - Object reference not set to an instance of an object.
            //commit: breakpoint refactorer / ActionDispatcher and TaskDispatcher (2026-05-25)
            if (!Navigation.IsPrerendering())
            {
                AppStateStatic.ProcessingStarted.Subscribe(async () =>
                {
                    Interlocked.Increment(ref _processingCount);
                    await InvokeAsync(StateHasChanged);
                }, Cts.Token);

                AppStateStatic.ProcessingFinished.Subscribe(async () =>
                {
                    Interlocked.Decrement(ref _processingCount);
                    await Task.Delay(200, Cts.Token);
                    await InvokeAsync(StateHasChanged);
                }, Cts.Token);
            }
        }

        private async Task OpenConfigurations()
        {
            _openMenu = false;
            await DialogService.SettingsPopup();
        }

        private Color GetColor(string endpoint)
        {
            return Focused(endpoint) ? Color.Primary : Color.Inherit;
        }

        private Variant GetVariant(string endpoint)
        {
            return Focused(endpoint) ? Variant.Filled : Variant.Text;
        }

        private bool Focused(string endpoint)
        {
            var uri = new Uri(Navigation.Uri);

            return string.Equals(uri.AbsolutePath, endpoint, StringComparison.OrdinalIgnoreCase);
        }

        private void MenuClick()
        {
            _openMenu = true;
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