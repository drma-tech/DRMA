using DRMA.API.Core;
using DRMA.API.Repository.Core;
using DRMA.Shared.Models.Support;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace DRMA.API.Repository
{
    public class CosmosEmailRepository
    {
        public Container Container { get; private set; }

        public CosmosEmailRepository(IConfiguration config)
        {
            var databaseId = config.GetValue<string>("CosmosDB:DatabaseId");

            Container = ApiStartup.CosmosClient.GetContainer(databaseId, "mail");
        }

        public async Task<EmailDocument?> Get(string id, CancellationToken cancellationToken)
        {
            try
            {
                var response = await Container.ReadItemAsync<EmailDocument?>(id, new PartitionKey(id), CosmosRepositoryExtensions.GetItemRequestOptions(), cancellationToken);

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

        public async Task<List<EmailDocument>> Query(Expression<Func<EmailDocument, bool>>? predicate, CancellationToken cancellationToken)
        {
            IQueryable<EmailDocument> query;

            if (predicate is null)
            {
                query = Container.GetItemLinqQueryable<EmailDocument>();
            }
            else
            {
                query = Container.GetItemLinqQueryable<EmailDocument>().Where(predicate);
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

        public async Task<EmailDocument?> UpsertItemAsync(EmailDocument email, CancellationToken cancellationToken)
        {
            var response = await Container.UpsertItemAsync(email, new PartitionKey(email.Id), CosmosRepositoryExtensions.GetItemRequestOptions(), cancellationToken);

            return response.Resource;
        }
    }
}