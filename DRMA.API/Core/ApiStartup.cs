using DRMA.Shared.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace DRMA.API.Core;

public static class ApiStartup
{
    public static HttpClient HttpClient { get; } = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip });

    public static HttpClient HttpClientPaddle { get; } = new();
    public static CosmosClient CosmosClient { get; private set; } = null!;
    public static Configurations Configurations { get; set; } = null!;

    public static void Startup(string? conn)
    {
        ArgumentNullException.ThrowIfNull(conn);

        CosmosClient = new CosmosClient(conn, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }

            //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-connection-modes
            //ConnectionMode = ConnectionMode.Gateway // ConnectionMode.Direct is the default
        });
    }
}