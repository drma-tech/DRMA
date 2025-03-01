using DRMA.Shared.Models.Auth;

namespace DRMA.WEB.Modules.Auth.Core
{
    public class LoginApi(IHttpClientFactory factory) : ApiCosmos<ClienteLogin>(factory, null)
    {
        private struct Endpoint
        {
            public static string Add(string platform, string? ip) => $"login/add?platform={platform}&ip={ip}";
        }

        public async Task Add(string platform)
        {
            var ip = "";

            try
            {
                //TODO: TypeError: Failed to fetch
                var response = await _http.GetAsync(new Uri("https://ipinfo.io/ip"));
                ip = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                //do nothing;
            }

            await PostAsync<ClienteLogin>(Endpoint.Add(platform, ip?.Trim()), null, null);
        }
    }
}