namespace DRMA.WEB.Core.Api;

public abstract class ApiExternal(IHttpClientFactory factory) : ApiCore(factory, null)
{
}