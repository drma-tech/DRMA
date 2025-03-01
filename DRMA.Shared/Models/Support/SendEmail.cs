namespace DRMA.Shared.Models.Support
{
    public class SendEmail
    {
        public string? Subject { get; set; }
        public string? Text { get; set; }
        public string? Html { get; set; }
        public string? FromName { get; set; }
        public string? FromEmail { get; set; }
        public string? ToName { get; set; }
        public string? ToEmail { get; set; }
    }
}