using Newtonsoft.Json;

namespace DRMA.Shared.Models.Support
{
    public class EmailDocument : CosmosDocument
    {
        public EmailDocument()
        {
        }

        public EmailDocument(string id) : base(id)
        {
        }

        public string? Subject { get; set; }
        public string? Html { get; set; }
        public string? Text { get; set; }
        public EmailAddress? From { get; set; }
        public List<EmailAddress> To { get; set; } = [];
        public List<EmailAddress> Cc { get; set; } = [];
        public DateTime? Date { get; set; }
        public string? SpamScore { get; set; }
        public string? SpamReport { get; set; }

        public string? SenderIp { get; set; }
        public bool Read { get; set; }
        public bool Replied { get; set; }

        [JsonIgnore]
        public string? FromName => From?.Name;

        [JsonIgnore]
        public string? FromEmail => From?.Email;

        [JsonIgnore]
        public string? ToName => To.FirstOrDefault()?.Name;

        [JsonIgnore]
        public string? ToEmail => To.FirstOrDefault()?.Email;

        public override bool Equals(object? obj)
        {
            return obj is EmailDocument q && q.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public override bool HasValidData()
        {
            return true;
        }
    }

    public class EmailAddress
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}