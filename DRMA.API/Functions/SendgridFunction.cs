using DRMA.Shared.Models.Support;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using StrongGrid;
using StrongGrid.Models;
using System.Globalization;

namespace DRMA.API.Functions
{
    public class SendgridFunction(CosmosEmailRepository repo, IConfiguration configuration)
    {
        [Function("GetEmails")]
        public async Task<List<EmailDocument>?> GetEmails(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.GET, Route = "adm/emails")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                return await repo.Query(null, cancellationToken);
            }
            catch (Exception ex)
            {
                req.ProcessException(ex);
                throw;
            }
        }

        [Function("EmailUpdate")]
        public async Task EmailUpdate(
           [HttpTrigger(AuthorizationLevel.Anonymous, Method.POST, Route = "adm/emails/update")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var Email = await req.GetPublicBody<EmailDocument>(cancellationToken);

                await repo.UpsertItemAsync(Email, cancellationToken);
            }
            catch (Exception ex)
            {
                req.ProcessException(ex);
                throw;
            }
        }

        [Function("SendgridInbound")]
        public async Task SendgridInbound(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.POST, Route = "public/sendgrid/inbound")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var parser = new WebhookParser();
                var inboundMail = await parser.ParseInboundEmailWebhookAsync(req.Body, cancellationToken);

                DateTime.TryParse(inboundMail.Headers.SingleOrDefault(w => w.Key == "Date").Value, CultureInfo.InvariantCulture, out DateTime date);

                var model = new EmailDocument(Guid.NewGuid().ToString())
                {
                    Subject = inboundMail.Subject,
                    Html = inboundMail.Html,
                    Text = inboundMail.Text,
                    From = new EmailAddress { Email = inboundMail.From.Email, Name = inboundMail.From.Name },
                    To = [.. inboundMail.To.Select(s => new EmailAddress { Email = s.Email, Name = s.Name })],
                    Cc = [.. inboundMail.Cc.Select(s => new EmailAddress { Email = s.Email, Name = s.Name })],
                    Date = date,
                    SenderIp = inboundMail.SenderIp,
                    SpamScore = inboundMail.SpamScore,
                    SpamReport = inboundMail.SpamReport
                };

                await repo.UpsertItemAsync(model, cancellationToken);
            }
            catch (Exception ex)
            {
                req.ProcessException(ex);
                throw;
            }
        }

        [Function("SendgridSendEmail")]
        public async Task SendgridSendEmail(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method.POST, Route = "adm/send-email")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var inbound = await req.GetPublicBody<SendEmail>(cancellationToken);

                var apiKey = configuration.GetValue<string>("Sendgrid:Key");
                var strongGridClient = new Client(apiKey);

                var textContent = new MailContent("text/plain", inbound.Text);
                //var htmlContent = new MailContent("text/html", inbound.Html);
                var from = new MailAddress(inbound.FromEmail, inbound.FromName);
                var to = new MailAddress(inbound.ToEmail, inbound.ToName);

                var personalization = new MailPersonalization
                {
                    From = from,
                    To = [to],
                    Subject = inbound.Subject,
                };

                await strongGridClient.Mail.SendAsync([personalization], inbound.Subject, [textContent], from, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                req.ProcessException(ex);
                throw;
            }
        }
    }
}