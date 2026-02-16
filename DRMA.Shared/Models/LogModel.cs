using System.Text.Json.Serialization;

namespace DRMA.Shared.Models
{
    public class LogModel
    {
        public string? Message { get; set; }
        public string? InnerException { get; set; }
        public string? StackTrace { get; set; }
        public string? Origin { get; set; } //route or function name
        public string? Params { get; set; } //query parameters or other context info
        public string? Body { get; set; }
        public string? OperationSystem { get; set; }
        public string? BrowserName { get; set; }
        public string? BrowserVersion { get; set; }
        public string? Platform { get; set; }
        public string? AppVersion { get; set; }
        public string? UserId { get; set; }
        public string? Ip { get; set; }
        public string? Country { get; set; }
        public string? UserAgent { get; set; }
        public bool? IsBot { get; set; }
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
        [JsonInclude] public int Ttl { get; init; } = (int)TtlCache.ThreeMonths;
    }
}
