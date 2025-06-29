using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace DRMA.API.Core;

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public IEnumerable<string> UserRoles { get; set; } = [];
}

public static class StaticWebAppsAuth
{
    private static readonly string[] Roles = ["anonymous"];
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static string GetUserId(this HttpRequestData req)
    {
        if (req.Url.Host.Contains("localhost"))
        {
            const string localId = "8ed6f45c90ac43248353b90a846a8519";

            return localId;
        }

        var principal = req.Parse();

        return principal?.Claims.FirstOrDefault(w => w.Type == ClaimTypes.NameIdentifier)?.Value ??
               throw new UnhandledException("user id not available");
    }

    private static ClaimsPrincipal? Parse(this HttpRequestData req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
        {
            var data = header.First();
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.ASCII.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, Options);
        }

        if (principal == null) return null;
        principal.UserRoles = principal.UserRoles.Except(Roles, StringComparer.CurrentCultureIgnoreCase);

        var principalUserRoles = principal.UserRoles.ToList();
        if (!principalUserRoles.Any()) return new ClaimsPrincipal();

        var identity = new ClaimsIdentity(principal.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails ?? ""));
        identity.AddClaims(principalUserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(identity);
    }
}
