namespace DRMA.Shared.Models.Auth
{
    public class ClienteLogin : PrivateMainDocument
    {
        public ClienteLogin() : base(DocumentType.Login)
        {
        }

        public string? UserId { get; set; }

        public Access[] Accesses { get; set; } = [];

        public override void Initialize(string userId)
        {
            base.Initialize(userId);
            UserId = userId;
        }

        public override bool HasValidData()
        {
            if (string.IsNullOrEmpty(UserId)) return false;

            return true;
        }
    }

    public class Access
    {
        public DateTimeOffset Date { get; set; }
        public string? Platform { get; set; }
        public string? Ip { get; set; }
    }
}