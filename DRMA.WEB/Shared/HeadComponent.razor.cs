using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DRMA.WEB.Shared
{
    public partial class HeadComponent
    {
        /// <summary>
        /// less than 70 characters
        /// </summary>
        [Parameter, EditorRequired] public string? Title { get; set; }

        [Parameter, EditorRequired] public string Url { get; set; }
        [Parameter, EditorRequired] public bool Index { get; set; }

        /// <summary>
        /// between 25 and 160 characters in length
        /// </summary>
        [Parameter] public string? Description { get; set; }

        /// <summary>
        /// optional, but absolute url is necessary if provided
        /// </summary>
        [Parameter] public string? Image { get; set; }

        [Parameter] public bool Shared { get; set; }

        [Parameter] public string? FixedAlternateLanguage { get; set; }
        [Parameter] public bool FixedTitle { get; set; }

        public string? Culture => AppStateStatic.GetCulture(Navigation);
        private string? TitleFull => $"{Title} | {AppInfo.Title}";
        private string? CanonicalUrl => $"{AppInfo.WebSite.TrimEnd('/')}/{Url.TrimStart('/')}";
        private string? ImageUrl => Image ?? $"{AppInfo.WebSite}/icon/icon-192.png";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !string.IsNullOrEmpty(Culture))
            {
                await Js.InvokeVoidAsync("setHtmlLang", Culture);
            }
        }

        private string GetUrlForLanguage(string lang)
        {
            var segments = Url.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            var path = segments.Length > 1 ? string.Join('/', segments.Skip(1)) : string.Empty;

            return $"{AppInfo.WebSite}/{lang}/{path}".TrimEnd('/');
        }
    }
}
