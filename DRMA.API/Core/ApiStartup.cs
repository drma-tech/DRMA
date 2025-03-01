using Microsoft.Azure.Cosmos;

namespace DRMA.API.Core
{
    public static class ApiStartup
    {
        public static HttpClient HttpClient { get; } = new(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
        public static HttpClient HttpClientPaddle { get; } = new();
        public static CosmosClient CosmosClient { get; private set; } = default!;

        public static void Startup(string conn)
        {
            CosmosClient = new(conn, new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },

                //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-connection-modes
                //ConnectionMode = ConnectionMode.Gateway // ConnectionMode.Direct is the default
            });
        }
    }
}