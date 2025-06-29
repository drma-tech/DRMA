using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DRMA.API.Core;

public static class IsolatedFunctionHelper
{
    public static async Task<T> GetBody<T>(this HttpRequestData req, CancellationToken cancellationToken)
        where T : CosmosDocument, new()
    {
        var model = await JsonSerializer.DeserializeAsync<T>(req.Body, cancellationToken: cancellationToken);

        model ??= new T();

        var userId = req.GetUserId();

        if (string.IsNullOrEmpty(userId)) throw new InvalidOperationException("unauthenticated user");

        if (model is ProtectedMainDocument prot)
            prot.Initialize(userId);
        else if (model is PrivateMainDocument priv)
            priv.Initialize(userId);
        else if (model is CosmosDocument doc) //generic document
            doc.SetIds(userId);

        return model;
    }

    public static async Task<T> GetPublicBody<T>(this HttpRequestData req, CancellationToken cancellationToken)
        where T : class, new()
    {
        req.Body.Position = 0; //in case of a previous read
        var model = await JsonSerializer.DeserializeAsync<T>(req.Body, cancellationToken: cancellationToken);
        model ??= new T();

        return model;
    }

    public static async Task<HttpResponseData> CreateResponse<T>(this HttpRequestData req, T? doc, TtlCache maxAge,
        CancellationToken cancellationToken) where T : class
    {
        HttpResponseData? response;

        if (doc != null)
        {
            response = req.CreateResponse();

            await response.WriteAsJsonAsync(doc, cancellationToken);
        }
        else
        {
            response = req.CreateResponse(HttpStatusCode.NoContent);
        }

        response.Headers.Add("Cache-Control", $"public, max-age={(int)maxAge}");
        //response.Headers.Add("ETag", eTag); // unique identification to verify data changes
        //response.Headers.Add("Access-Control-Expose-Headers", "ETag"); //don't using anymore

        return response;
    }

    public static StringDictionary GetQueryParameters(this HttpRequestData req)
    {
        var valueCollection = HttpUtility.ParseQueryString(req.Url.Query);

        var dictionary = new StringDictionary();
        foreach (var key in valueCollection.AllKeys)
            if (key != null)
                dictionary.Add(key.ToLowerInvariant(), valueCollection[key]);

        return dictionary;
    }

    public static void ProcessException(this HttpRequestData req, Exception ex)
    {
        var logger = req.FunctionContext.GetLogger(req.FunctionContext.FunctionDefinition.Name);

        const string messageTemplate = "ProcessException. State: {State}, Params: {Params}";

        logger.LogError(ex, messageTemplate, req.BuildState(), req.BuildParams());
    }

    public static void LogWarning(this HttpRequestData req, string? message)
    {
        var logger = req.FunctionContext.GetLogger(req.FunctionContext.FunctionDefinition.Name);

        const string messageTemplate = "LogWarning. Message: {message} State: {State}, Params: {Params}";

        logger.LogWarning(messageTemplate, message, req.BuildState(), req.BuildParams());
    }

    private static string BuildState(this HttpRequestData req)
    {
        var valueCollection = HttpUtility.ParseQueryString(req.Url.Query);

        return string.Join("",
            valueCollection.AllKeys.Select(key => $"{key?.ToLowerInvariant()}={{{key?.ToLowerInvariant()}}}|"));
    }

    private static string[] BuildParams(this HttpRequestData req)
    {
        var valueCollection = HttpUtility.ParseQueryString(req.Url.Query);

        return valueCollection.AllKeys.Select(key => valueCollection[key] ?? "").ToArray();
    }
}

public struct Method
{
    public const string Get = "GET";

    public const string Post = "POST";

    public const string Put = "PUT";

    public const string Delete = "DELETE";
}