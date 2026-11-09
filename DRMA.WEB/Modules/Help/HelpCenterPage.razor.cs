namespace DRMA.WEB.Modules.Help
{
    public partial class HelpCenterPage
    {
        private async Task ShowCacheClick()
        {
            try
            {
                await JsRuntime.Utils().ShowCache(Cts.Token);
            }
            catch (Exception ex)
            {
                await ProcessException(ex);
            }
        }

        private void ShowConsole()
        {
            Navigation.NavigateTo("w2n://console-screen");
        }

        private async Task ClearCacheClick()
        {
            try
            {
                await JsRuntime.Utils().ClearAllStorage();
            }
            catch (Exception ex)
            {
                await ProcessException(ex);
            }
        }

        private void AppLanguageClick(AppLanguage lang)
        {
            Navigation.NavigateTo($"/{lang}/help", forceLoad: true);
        }
    }
}
