using DRMA.API.Repository.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace DRMA.API.Repository
{
    public class LogModel
    {
        public LogModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public DateTimeOffset DateTimeError { get; set; } = DateTimeOffset.Now;

        [JsonInclude]
        public int Ttl { get; init; }
    }

    public class CosmosLogRepository
    {
        public Container Container { get; private set; }

        public CosmosLogRepository(IConfiguration config)
        {
            var conn = config.GetValue<string>("Product:DRMA:CosmosDB:ConnectionString");
            var databaseId = config.GetValue<string>("Product:DRMA:CosmosDB:DatabaseId");

            Container = ApiStartup.CosmosClient(conn).GetContainer(databaseId, "logs");
        }

        public async Task Add(LogModel log)
        {
            await Container.CreateItemAsync(log, new PartitionKey(log.Id), CosmosRepositoryExtensions.GetItemRequestOptions());
        }
    }
}