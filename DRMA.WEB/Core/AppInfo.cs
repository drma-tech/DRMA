using DRMA.WEB.Modules.Support.Core;

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
            new("Streaming Discovery", "Discover Movies and Series on Streaming Platforms", "https://www.streamingdiscovery.com", "/logo/streamingdiscovery.webp", true ),
            new("Modern Matchmaker", "Find a Compatible Partner Through Smart Matching", "https://www.modern-matchmaker.com", "/logo/modern-matchmaker.webp", true ),
            new("My Next Spot", "Find the Best Cities and Countries to Live or Travel", "https://www.my-next-spot.com", "/logo/next-spot.webp", false ),
            new("WebStandards", "Web Standards Generator for Websites and PWAs", "https://www.web-standards.com", "/logo/webstandards.webp", false ),
       ];
    }
}
