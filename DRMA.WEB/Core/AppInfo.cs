using DRMA.WEB.Modules.Help.Core;

namespace DRMA.WEB.Core
{
    public static class AppInfo
    {
        public static string CompanyName { get; set; } = "DRMA Tech";
        public static string CompanyWebSite { get; set; } = $"https://www.drma-tech.com";

        public static string Title { get; set; } = "DRMA Tech";
        public static string Subtitle { get; set; } = "Delivering Reliable Modern Apps";
        public static string Domain { get; set; } = "drma-tech";
        public static string WebSite { get; set; } = $"https://www.{Domain}.com";
        public static int Year { get; set; } = 2025;

        public static readonly ProductLink[] Products =
        [
            new("Streaming Discovery", Translations.Product.streamingdiscovery, Translations.Product.streamingdiscovery_desc, "https://streamingdiscovery.com", "/logo/streamingdiscovery.png", live: true ),
            new("Modern Matchmaker", Translations.Product.modern_matchmaker, Translations.Product.modern_matchmaker_desc, "https://modern-matchmaker.com", "/logo/modern-matchmaker.png", live: true ),
            new("My Next Spot", Translations.Product.next_spot, Translations.Product.next_spot_desc, "https://my-next-spot.com", "/logo/next-spot.png", live: true ),
            new("Web Standards", Translations.Product.webstandards, Translations.Product.webstandards_desc, "https://web-standards.com", "/logo/webstandards.png", live: false ),
            //new("Shared Home", Translations.Product.shared_home, Translations.Product.shared_home_desc, "https://shared-home.com", "/logo/shared-home.png", live: true ),
       ];
    }
}
