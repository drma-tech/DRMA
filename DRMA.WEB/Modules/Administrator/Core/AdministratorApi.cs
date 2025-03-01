using DRMA.Shared.Models.Support;
using DRMA.WEB.Core.Api;

namespace DRMA.WEB.Modules.Administrator.Core
{
    public class AdministratorApi(IHttpClientFactory factory) : ApiCosmos<EmailDocument>(factory, null)
    {
        private struct Endpoint
        {
            public const string GetEmails = "adm/emails";
            public const string EmailUpdate = "adm/emails/update";
            public const string SendEmail = "adm/send-email";
        }

        public async Task<HashSet<EmailDocument>> GetEmails()
        {
            return await GetListAsync(Endpoint.GetEmails, null);
        }

        public async Task SendEmail(SendEmail inbound)
        {
            await PostAsync(Endpoint.SendEmail, null, inbound);
        }

        public async Task EmailUpdate(EmailDocument email)
        {
            await PostAsync(Endpoint.EmailUpdate, null, email);
        }
    }
}