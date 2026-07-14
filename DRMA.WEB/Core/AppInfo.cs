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
            new("Streaming Discovery", "Discover Movies and Series on Streaming Platforms", "https://streamingdiscovery.com", "/logo/streamingdiscovery.png", true ),
            new("Modern Matchmaker", "Find a compatible partner through Smart Matchmaking", "https://modern-matchmaker.com", "/logo/modern-matchmaker.png", true ),
            new("My Next Spot", "Find the Best Cities and Countries to Live or Travel", "https://my-next-spot.com", "/logo/next-spot.png", true ),
            new("Web Standards", "Web Standards Generator for Websites and PWAs", "https://web-standards.com", "/logo/webstandards.png", false ),
       ];
    }
}