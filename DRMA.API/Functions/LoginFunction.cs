using DRMA.API.Core.Auth;
using DRMA.Shared.Core.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace DRMA.API.Functions;

public class LoginFunction(IHttpClientFactory factory)
{
    [Function("Test")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = "public/test")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("OK");
        return response;
    }

    [Function("Logger")]
    public static async Task<IActionResult> Logger([HttpTrigger(AuthorizationLevel.Anonymous, Method.Post, Route = "public/logger")] HttpRequestData req, CancellationToken cancellationToken)
    {
        try
        {
            if (req.Method == "OPTIONS")
            {
                return new OkResult();
            }

            var log = await req.GetPublicBody<LogModel>(cancellationToken);

            req.LogError(null, null, log);

            return new OkResult();
        }
        catch (Exception)
        {
            req.LogError(null, null, null);
            return new OkResult();
        }
    }

    [Function("Country")]
    public async Task<string?> Country([HttpTrigger(AuthorizationLevel.Anonymous, Method.Get, Route = "public/country")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var ip = req.GetUserIP();
        if (ip == "127.0.0.1") ip = "8.8.8.8";
        var client = factory.CreateClient("ipinfo");

        var result = await client.GetValueAsync($"https://ipinfo.io/{ip}/country", cancellationToken);

        return result;
    }
}