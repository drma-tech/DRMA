namespace DRMA.WEB.Modules.Subscription.Core
{
    public class IpInfoApi(IHttpClientFactory factory) : ApiExternal(factory)
    {
        public async Task<string?> GetCountry()
        {
            try
            {
                return await GetValueAsync("https://ipinfo.io/country");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class LoggerApi(IHttpClientFactory factory) : ApiCore(factory, null, ApiType.Anonymous)
    {
        public async Task SaveLog(LogModel log)
        {
            await PostAsync<LogModel, LogModel>("public/logger", log);
        }
    }
}