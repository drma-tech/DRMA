namespace DRMA.WEB.Layout
{
    public partial class FooterComponent : IDisposable
    {
        private string Culture => Navigation.GetCulture();

        protected CancellationTokenSource Cts { get; } = new();

        protected override void OnInitialized()
        {
            AppStateStatic.BreakpointChanged.Subscribe(breakpoint => StateHasChanged(), Cts.Token);
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