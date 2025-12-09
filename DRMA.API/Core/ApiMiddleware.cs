using DRMA.API.Core;
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
            //todo: validate platform version
            //var req = await context.GetHttpRequestDataAsync();
            //if (req != null)
            //{
            //    var path = req.Url.AbsolutePath;
            //    var ignoredPaths = new[]
            //    {
            //        "/api/principal/get",
            //    };

            //    if (ignoredPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
            //    {
            //        await next(context);
            //    }
            //    else
            //    {
            //        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            //        var vs = query["vs"];

            //        if (vs.NotEmpty() && DateTime.Parse(vs, CultureInfo.InvariantCulture).Date < DateTime.Now.AddDays(1))
            //        {
            //            throw new NotificationException("Old platform version. Please update.");
            //        }
            //    }
            //}
            //else
            //{
            //    await next(context);
            //}

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