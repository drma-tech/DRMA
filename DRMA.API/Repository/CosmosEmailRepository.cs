using DRMA.API.Repository.Core;
using DRMA.Shared.Models.Support;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace DRMA.API.Repository
{
    public class CosmosEmailRepository(IConfiguration config)
    {
        private IConfiguration Config { get; set; } = config;

        public async Task<EmailDocument?> Get(string product, string id, CancellationToken cancellationToken)
        {
            try
            {
                var conn = Config.GetValue<string>($"Product:{product}:CosmosDB:ConnectionString");
                var databaseId = Config.GetValue<string>($"Product:{product}:CosmosDB:DatabaseId");
                var container = ApiStartup.CosmosClient(conn).GetContainer(databaseId, "mail");

                var response = await container.ReadItemAsync<EmailDocument?>(id, new PartitionKey(id), CosmosRepositoryExtensions.GetItemRequestOptions(), cancellationToken);

                return response.Resource;
            }
            catch (CosmosOperationCanceledException)
            {
                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<EmailDocument>> Query(string product, Expression<Func<EmailDocument, bool>>? predicate, CancellationToken cancellationToken)
        {
            var conn = Config.GetValue<string>($"Product:{product}:CosmosDB:ConnectionString");
            var databaseId = Config.GetValue<string>($"Product:{product}:CosmosDB:DatabaseId");
            var container = ApiStartup.CosmosClient(conn).GetContainer(databaseId, "mail");

            IQueryable<EmailDocument> query;

            if (predicate is null)
            {
                query = container.GetItemLinqQueryable<EmailDocument>();
            }
            else
            {
                query = container.GetItemLinqQueryable<EmailDocument>().Where(predicate);
            }

            using var iterator = query.ToFeedIterator();
            var results = new List<EmailDocument>();
            double count = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);

                count += response.RequestCharge;

                results.AddRange(response.Resource);
            }

            return results;
        }

        public async Task<EmailDocument?> UpsertItemAsync(string product, EmailDocument email, CancellationToken cancellationToken)
        {
            var conn = Config.GetValue<string>($"Product:{product}:CosmosDB:ConnectionString");
            var databaseId = Config.GetValue<string>($"Product:{product}:CosmosDB:DatabaseId");
            var container = ApiStartup.CosmosClient(conn).GetContainer(databaseId, "mail");

            var response = await container.UpsertItemAsync(email, new PartitionKey(email.Id), CosmosRepositoryExtensions.GetItemRequestOptions(), cancellationToken);

            return response.Resource;
        }
    }
}