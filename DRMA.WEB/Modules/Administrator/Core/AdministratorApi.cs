using DRMA.Shared.Models.Support;

namespace DRMA.WEB.Modules.Administrator.Core
{
    public class AdministratorApi(IHttpClientFactory factory) : ApiCosmos<EmailDocument>(factory, null)
    {
        private struct Endpoint
        {
            public static string GetEmails(string? product) => $"adm/emails/{product}";

            public static string EmailUpdate(string? product) => $"adm/emails/update/{product}";

            public static string SendEmail(string? product) => $"adm/send-email/{product}";
        }

        public async Task<HashSet<EmailDocument>> GetEmails(string? product)
        {
            return await GetListAsync(Endpoint.GetEmails(product), null);
        }

        public async Task SendEmail(SendEmail inbound, string? product)
        {
            await PostAsync(Endpoint.SendEmail(product), null, inbound);
        }

        public async Task EmailUpdate(EmailDocument email, string? product)
        {
            await PostAsync(Endpoint.EmailUpdate(product), null, email);
        }
    }
}