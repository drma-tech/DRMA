namespace DRMA.WEB.Core.Api;

public abstract class ApiExternal(IHttpClientFactory factory) : ApiCore(factory, null, ApiType.Anonymous)
{
    protected async new Task<T?> GetAsync<T>(string uri, bool setNewVersion, CancellationToken cancellationToken) where T : class
    {
        return await base.GetAsync<T>($"public/external?url=" + uri.ConvertFromStringToBase64(), setNewVersion, cancellationToken);
    }
}
