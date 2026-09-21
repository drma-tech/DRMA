using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DRMA.WEB.Shared
{
    public partial class PageSectionComponent
    {
        //Header
        [Parameter] public bool IsPageHeader { get; set; } = false;

        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Subtitle { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public string? Link { get; set; }
        [Parameter] public bool ShowHeader { get; set; } = true;
        [Parameter] public bool ShowActions { get; set; } = true;
        [Parameter] public string? UrlShare { get; set; }

        //Layout
        [Parameter] public Color Color { get; set; } = Color.Primary;

        [Parameter] public string? Icon { get; set; }
        [Parameter] public string? Image { get; set; }
        [Parameter] public string? ImageStyle { get; set; }
        [Parameter] public bool Visible { get; set; } = true;
        [Parameter] public string? CustomBodyClass { get; set; }
        [Parameter] public bool HasBodyBox { get; set; } = true;

        //Fragments
        [Parameter] public RenderFragment? ExtraFragment { get; set; }

        [Parameter] public RenderFragment? ActionsFragment { get; set; }
        [Parameter] public RenderFragment? BodyFragment { get; set; }

        public string? BodyClass { get; set; }

        private string? AbsolutelUrl => $"{AppInfo.WebSite.TrimEnd('/')}/{UrlShare?.TrimStart('/')}";

        private static string headerImageSize => AppStateStatic.IsMobile ? "40px" : "48px";
        private static string sectionImageSize => AppStateStatic.IsMobile ? "20px" : "24px";
        private static string titleFontSize => AppStateStatic.IsMobile ? "20px" : "24px";
        private static string subtitleFontSize => AppStateStatic.IsMobile ? "13.5px" : "16px";

        private bool HasActions => ActionsFragment != null;
        private bool HasBody => BodyFragment != null;

        protected override void OnInitialized()
        {
            if (CustomBodyClass.Empty())
            {
                BodyClass = CssHelper.Build().Large("mb");
                AppStateStatic.BreakpointChanged.Subscribe(breakpoint => BodyClass = CssHelper.Build().Large("mb"), CancellationToken.None);
            }
            else
            {
                BodyClass = CustomBodyClass;
            }
        }

        private string GetImageStyle(bool header)
        {
            return ImageStyle ?? $"max-height: {(header ? headerImageSize : sectionImageSize)}; max-width: {(header ? headerImageSize : sectionImageSize)}; width: 100%; vertical-align: top;";
        }
    }
}
