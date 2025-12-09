using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var app = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ApiMiddleware>();
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        try
        {
            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                config.AddJsonFile("local.settings.json");
                config.AddUserSecrets<Program>();
            }

            var cfg = new Configurations();
            config.Build().Bind(cfg);
            ApiStartup.Configurations = cfg;
        }
        catch (Exception ex)
        {
            var tempClient = new CosmosClient(ApiStartup.Configurations.CosmosDB?.ConnectionString, new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
            var tempRepo = new CosmosLogRepository(tempClient);
            var provider = new CosmosLoggerProvider(tempRepo);
            var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
            var logger = loggerFactory.CreateLogger("ConfigureAppConfiguration");

            logger.LogError(ex, "ConfigureAppConfiguration");
        }
    })
    .ConfigureServices(ConfigureServices)
    .Build();

await app.RunAsync();

static void ConfigureServices(IServiceCollection services)
{
    try
    {
        //http clients

        services.AddHttpClient("ipinfo");

        //repositories

        services.AddSingleton(provider =>
        {
            return new CosmosClient(ApiStartup.Configurations.CosmosDB?.ConnectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        });

        services.AddSingleton<CosmosLogRepository>();

        services.AddSingleton<ILoggerProvider>(provider =>
        {
            var repo = provider.GetRequiredService<CosmosLogRepository>();
            return new CosmosLoggerProvider(repo);
        });

        //general services

        services.AddDistributedMemoryCache();
    }
    catch (Exception ex)
    {
        var tempClient = new CosmosClient(ApiStartup.Configurations.CosmosDB?.ConnectionString, new CosmosClientOptions()
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        });
        var tempRepo = new CosmosLogRepository(tempClient);
        var provider = new CosmosLoggerProvider(tempRepo);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var logger = loggerFactory.CreateLogger("ConfigureServices");

        logger.LogError(ex, "ConfigureServices");
    }
}