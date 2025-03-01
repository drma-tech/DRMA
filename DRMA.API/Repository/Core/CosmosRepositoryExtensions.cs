using Microsoft.Azure.Cosmos;

namespace DRMA.API.Repository.Core
{
    public static class CosmosRepositoryExtensions
    {
        public static ItemRequestOptions? GetItemRequestOptions()
        {
            return new ItemRequestOptions
            {
                //to this work, the changes need to be made by frontend
                //EnableContentResponseOnWrite = false
            };
        }

        public static PatchItemRequestOptions? GetPatchItemRequestOptions()
        {
            return new PatchItemRequestOptions
            {
                //to this work, the changes need to be made by frontend
                //EnableContentResponseOnWrite = false
            };
        }

        public static QueryRequestOptions? GetQueryRequestOptions()
        {
            return new QueryRequestOptions()
            {
                //https://learn.microsoft.com/en-us/training/modules/measure-index-azure-cosmos-db-sql-api/4-measure-query-cost
                MaxItemCount = 10, // - max itens per page
            };
        }
    }
}