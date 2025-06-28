using System.Text.Json.Serialization;
using DRMA.API.Repository.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace DRMA.API.Repository;

public class LogModel
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public DateTimeOffset DateTimeError { get; set; } = DateTimeOffset.Now;

    [JsonInclude] public int Ttl { get; init; }
}

public class CosmosLogRepository
{
    public CosmosLogRepository(IConfiguration config)
    {
        var conn = config.GetValue<string>("Product:DRMA:CosmosDB:ConnectionString");
        var databaseId = config.GetValue<string>("Product:DRMA:CosmosDB:DatabaseId");

        Container = ApiStartup.CosmosClient(conn).GetContainer(databaseId, "logs");
    }

    public Container Container { get; }

    public async Task Add(LogModel log)
    {
        await Container.CreateItemAsync(log, new PartitionKey(log.Id),
            CosmosRepositoryExtensions.GetItemRequestOptions());
    }
}