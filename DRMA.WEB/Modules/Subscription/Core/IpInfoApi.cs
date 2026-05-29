namespace DRMA.WEB.Modules.Subscription.Core
{
    public class IpInfoApi(IHttpClientFactory factory) : ApiExternal(factory)
    {
        public async Task<string?> GetCountry(CancellationToken cancellationToken)
        {
            try
            {
                return await GetStringAsync("public/country", cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
