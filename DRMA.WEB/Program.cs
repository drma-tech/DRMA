using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseSentry(options =>
{
    options.Dsn = "https://9d67a029559a76d507d4437b586e778f@o4510938040041472.ingest.us.sentry.io/4510942795005952";
    options.DiagnosticLevel = SentryLevel.Warning;

    options.TracePropagationTargets = []; //Disable tracing because it breaks communication with external APIs.

    options.SetBeforeSend(evt =>
    {
        evt.SetTag("custom.version", AppStateStatic.Version ?? "error");
        evt.SetTag("custom.platform", AppStateStatic.GetSavedPlatform()?.ToString() ?? "error");

        evt.SetExtra("browser_name", AppStateStatic.BrowserName);
        evt.SetExtra("browser_version", AppStateStatic.BrowserVersion);
        evt.SetExtra("operation_system", AppStateStatic.OperatingSystem);

        return evt;
    });
});

builder.Logging.SetMinimumLevel(LogLevel.Warning);

if (builder.RootComponents.Empty())
{
    builder.RootComponents.Add<DRMA.WEB.App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
}

ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress, builder.Configuration);

var app = builder.Build();

var js = app.Services.GetRequiredService<IJSRuntime>();

AppStateStatic.Version = await AppStateStatic.GetAppVersion(js);
AppStateStatic.BrowserName = await js.Utils().GetBrowserName();
AppStateStatic.BrowserVersion = await js.Utils().GetBrowserVersion();
AppStateStatic.OperatingSystem = await js.Utils().GetOperatingSystem();
AppStateStatic.UserAgent = await js.Window().InvokeAsync<string>("eval", "navigator.userAgent");

await js.Utils().SetStorage("app-version", AppStateStatic.Version);
await AppStateStatic.GetPlatform(js);
await js.Services().InitGoogleAnalytics(AppStateStatic.Version);

await app.RunAsync();

static void ConfigureServices(IServiceCollection collection, string baseAddress, IConfiguration configuration)
{
    collection.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
    });

    collection.AddPWAUpdater();

    collection.AddHttpClient("Local", c => { c.BaseAddress = new Uri(baseAddress); });

    var apiOrigin = configuration["DownstreamApi:BaseUrl"] ??
        (baseAddress.Contains("localhost") || baseAddress.Contains("127.0.0.1") ? throw new UnhandledException($"DownstreamApi:BaseUrl is null.") : $"{baseAddress}api/");

    collection.AddHttpClient("Anonymous", (service, options) => { options.BaseAddress = new Uri(apiOrigin); options.Timeout = TimeSpan.FromSeconds(60); })
       .AddPolicyHandler(request => request.Method == HttpMethod.Get ? GetRetryPolicy() : Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>());

    collection.AddHttpClient("External", (service, options) => { options.Timeout = TimeSpan.FromSeconds(60); })
        .AddPolicyHandler(request => request.Method == HttpMethod.Get ? GetRetryPolicy() : Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>());

    collection.AddAuthorizationCore();
}

//https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 408,5xx
        .WaitAndRetryAsync([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)]);
}