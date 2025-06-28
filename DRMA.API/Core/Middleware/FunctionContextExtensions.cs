using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DRMA.API.Core.Middleware;

public static class FunctionContextExtensions
{
    public static async Task SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode,
        string message)
    {
        var req = await context.GetHttpRequestDataAsync();

        var response = req!.CreateResponse(statusCode);

        await response.WriteStringAsync(message);

        var invocationResult = context.GetInvocationResult();

        var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
        if (httpOutputBindingFromMultipleOutputBindings is not null)
            httpOutputBindingFromMultipleOutputBindings.Value = response;
        else
            invocationResult.Value = response;
    }

    private static OutputBindingData<HttpResponseData>? GetHttpOutputBindingFromMultipleOutputBinding(
        FunctionContext context)
    {
        // The output binding entry name will be "$return" only when the function return type is HttpResponseData
        var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

        return httpOutputBinding;
    }
}