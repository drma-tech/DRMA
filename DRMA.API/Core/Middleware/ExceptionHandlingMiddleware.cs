using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DRMA.API.Core.Middleware
{
    internal sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (CosmosOperationCanceledException)
            {
                await context.SetHttpResponseStatusCode(HttpStatusCode.RequestTimeout, "Request Timeout!");
            }
            catch (CosmosException)
            {
                //var result = JsonSerializer.Deserialize<CosmosExceptionStructure>("{" + cex.ResponseBody.Replace("Errors", "\"Errors\"") + "}", options: null);
                //return result?.Message?.Errors.FirstOrDefault();
                await context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError, "Invocation failed!");
            }
            catch (NotificationException ex)
            {
                await context.SetHttpResponseStatusCode(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExceptionHandlingMiddleware");

                await context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError, "Invocation failed!");
            }
        }
    }
}