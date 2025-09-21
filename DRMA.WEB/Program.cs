using DRMA.WEB;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

if (builder.RootComponents.Empty())
{
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
}

ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);

var app = builder.Build();

var js = app.Services.GetRequiredService<IJSRuntime>();

//var version = DRMA.WEB.Layout.MainLayout.GetAppVersion();
//await js.InvokeVoidAsync("initGoogleAnalytics", "G-4BSYH92X9W", version);

await app.RunAsync();

static void ConfigureServices(IServiceCollection collection, string baseAddress)
{
    collection.AddMudServices();

    collection.AddPWAUpdater();

    collection.AddHttpClient("LocalHttpClient", c => { c.BaseAddress = new Uri(baseAddress); });

    collection.AddHttpClient("ExternalHttpClient")
        .AddPolicyHandler(request => request.Method == HttpMethod.Get ? GetRetryPolicy() : Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>());

    collection.AddOptions();
    collection.AddAuthorizationCore();
}

//https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 408,5xx
        .WaitAndRetryAsync([TimeSpan.FromSeconds(2)]);
}