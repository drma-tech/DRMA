using DRMA.WEB.Modules.Support.Core;

namespace DRMA.WEB.Core
{
    public static class AppInfo
    {
        public static string CompanyName { get; set; } = "DRMA Tech";
        public static string CompanyWebSite { get; set; } = $"https://www.drma-tech.com";

        public static int Year { get; set; } = 2025;

        public static readonly ProductLink[] Products =
        [
            new("Streaming Discovery", "Manage, Track, Discover", "https://www.streamingdiscovery.com", "/logo/streamingdiscovery.png", true ),
            new("Modern Matchmaker", "Find your life partner", "https://www.modern-matchmaker.com", "/logo/modern-matchmaker.png", true ),
            new("My Next Spot", "Match your next destination", "https://www.my-next-spot.com", "/logo/next-spot.png", false ),
            new("WebStandards", "Web Developer Tools", "https://www.web-standards.com", "/logo/webstandards.png", false ),
       ];
    }
}
