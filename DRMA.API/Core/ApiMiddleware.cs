using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Diagnostics;
using System.Net;

namespace DRMA.API.Core;

internal sealed class ApiMiddleware() : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        catch (CosmosOperationCanceledException ex)
        {
            var req = await context.GetHttpRequestDataAsync();
            req?.LogError(ex, "ApiMiddleware - CosmosOperationCanceledException");
            await context.SetHttpResponseStatusCode(HttpStatusCode.RequestTimeout, "Cosmos Request Timeout!");
        }
        catch (CosmosException ex)
        {
            var req = await context.GetHttpRequestDataAsync();
            req?.LogError(ex, "ApiMiddleware - CosmosException");
            await context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError, "Invocation failed!");
        }
        catch (NotificationException ex)
        {
            await context.SetHttpResponseStatusCode(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            if (ex.CancellationToken.IsCancellationRequested)
                await context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError, "Invocation cancelled!");
            else
                await context.SetHttpResponseStatusCode(HttpStatusCode.RequestTimeout, "Request Timeout!");
        }
        catch (Exception ex)
        {
            var req = await context.GetHttpRequestDataAsync();
            req?.LogError(ex, "ApiMiddleware - Exception");
            await context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError, "Invocation failed!");
        }
        finally
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds > 5000)
            {
                var req = await context.GetHttpRequestDataAsync();
                req?.LogWarning($"Executed in {sw.Elapsed}");
            }
        }
    }
}
