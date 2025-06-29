using DRMA.API.Repository.Core;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace DRMA.API.Repository;

public class LogModel
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;

    [JsonInclude] public int Ttl { get; init; }
}

public class CosmosLogRepository()
{
    public Container Container { get; } = ApiStartup.CosmosClient.GetContainer(ApiStartup.Configurations.CosmosDB?.DatabaseId, "logs");

    public async Task Add(LogModel log)
    {
        await Container.CreateItemAsync(log, new PartitionKey(log.Id), CosmosRepositoryExtensions.GetItemRequestOptions());
    }
}