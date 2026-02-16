using DRMA.API.Repository.Core;
using DRMA.Shared.Core.Helper;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Text.Json.Serialization;

namespace DRMA.API.Repository;

public class LogDbModel
{
    public string? Id { get; set; }
    public string? OperationSystem { get; set; }
    public string? BrowserName { get; set; }
    public string? BrowserVersion { get; set; }
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public string? UserId { get; set; }
    public string? Country { get; set; }
    public string? UserAgent { get; set; }
    public bool? IsBot { get; set; }
    public List<LogDbEvent> Events { get; set; } = [];
    [JsonInclude] public int Ttl { get; init; } = (int)TtlCache.ThreeMonths;
}

public class LogDbEvent
{
    public string? Message { get; set; }
    public string? InnerException { get; set; }
    public string? StackTrace { get; set; }
    public string? Origin { get; set; } //route or function name
    public string? Params { get; set; } //query parameters or other context info
    public string? Body { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;
}

public class CosmosLogRepository
{
    public Container Container { get; }

    public CosmosLogRepository(CosmosClient CosmosClient)
    {
        var databaseId = ApiStartup.Configurations.CosmosDB?.DatabaseId;

        Container = CosmosClient.GetContainer(databaseId, "logs");
    }

    public async Task Add(LogModel log)
    {
        var id = $"{log.Ip ?? "null-ip"}_{log.UserAgent.ToHash() ?? "null-ua"}";
        var pk = new PartitionKey(id);

        const int maxRetries = 10;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            LogDbModel dbModel;
            string? etag = null;

            try
            {
                var response = await Container.ReadItemAsync<LogDbModel>(id, pk, CosmosRepositoryExtensions.GetItemRequestOptions());

                dbModel = response.Resource;
                etag = response.ETag;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                dbModel = new LogDbModel
                {
                    Id = id,
                    OperationSystem = log.OperationSystem,
                    BrowserName = log.BrowserName,
                    BrowserVersion = log.BrowserVersion,
                    Platform = log.Platform,
                    AppVersion = log.AppVersion,
                    UserId = log.UserId,
                    Country = log.Country,
                    UserAgent = log.UserAgent,
                    IsBot = log.IsBot,
                };
            }

            dbModel.Events.Add(new LogDbEvent
            {
                Message = log.Message,
                InnerException = log.InnerException,
                StackTrace = log.StackTrace,
                Origin = log.Origin,
                Params = log.Params,
                Body = log.Body,
                DateTime = log.DateTime
            });

            dbModel.Events = dbModel.Events.OrderBy(e => e.DateTime).ToList();

            try
            {
                var requestOptions = CosmosRepositoryExtensions.GetItemRequestOptions();

                if (etag == null)
                {
                    await Container.CreateItemAsync(dbModel, pk, requestOptions);
                }
                else
                {
                    requestOptions.IfMatchEtag = etag;
                    await Container.ReplaceItemAsync(dbModel, id, pk, requestOptions);
                }

                return;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                await Task.Delay(50 * (int)Math.Pow(2, attempt - 1)); // backoff
            }
        }
    }
}
