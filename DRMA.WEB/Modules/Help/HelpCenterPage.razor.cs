namespace DRMA.WEB.Modules.Help
{
    public partial class HelpCenterPage
    {
        private async Task ShowCacheClick()
        {
            await JsRuntime.Utils().ShowCache(Cts.Token);
        }

        private async Task ClearCacheClick()
        {
            await JsRuntime.Utils().ClearAllStorage();
        }

        private void AppLanguageClick(AppLanguage lang)
        {
            Navigation.NavigateTo($"/{lang}/help", forceLoad: true);
        }
    }
}