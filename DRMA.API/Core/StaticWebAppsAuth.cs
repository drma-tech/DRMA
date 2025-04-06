using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DRMA.API.Core
{
    public class ClientPrincipal
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        public IEnumerable<string> UserRoles { get; set; } = [];
    }

    public static class StaticWebAppsAuth
    {
        public static string GetUserId(this HttpRequestData req)
        {
            if (req.Url.Host.Contains("localhost"))
            {
                var local_id = "b9a10c8be2f244c0a625b78f05e30812";

                return local_id;
            }
            else
            {
                var principal = req.Parse();

                return principal?.Claims.FirstOrDefault(w => w.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new NotificationException("user id not available");
            }
        }

        private static readonly string[] roles = ["anonymous"];
        private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        private static ClaimsPrincipal? Parse(this HttpRequestData req)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
            {
                var data = header.First();
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.ASCII.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, options);
            }

            if (principal != null)
            {
                principal.UserRoles = principal.UserRoles?.Except(roles, StringComparer.CurrentCultureIgnoreCase) ?? [];

                if (!principal.UserRoles?.Any() ?? true)
                {
                    return new ClaimsPrincipal();
                }

                var identity = new ClaimsIdentity(principal.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId ?? ""));
                identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails ?? ""));
                identity.AddClaims(principal.UserRoles?.Select(r => new Claim(ClaimTypes.Role, r)) ?? []);

                return new ClaimsPrincipal(identity);
            }
            else
            {
                return null;
            }
        }
    }
}
