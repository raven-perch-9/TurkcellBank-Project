using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using TurkcellBank.Client;
using TurkcellBank.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 250;
    config.SnackbarConfiguration.ShowTransitionDuration = 250;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

var baseAddress = builder.HostEnvironment.BaseAddress;
var isLocal = baseAddress.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase);
var isProd = baseAddress.EndsWith("azurewebsites.net", StringComparison.OrdinalIgnoreCase);
var effectiveEnv = isLocal ? "Development" : 
                     isProd ? "Production" : 
                     builder.HostEnvironment.Environment;

var http = new HttpClient { BaseAddress = new Uri(baseAddress) };

var config = new ConfigurationBuilder()
    .AddJsonStream(await http.GetStreamAsync("appsettings.json"))
    .Build();

var envFile = $"appsettings.{effectiveEnv}.json";
try
{
    using var envStream = await http.GetStreamAsync(envFile);
    config = new ConfigurationBuilder().AddConfiguration(config).AddJsonStream(envStream).Build();
}
catch
{
    // Env File Later
}

string apiBaseUrl = config["ApiBaseUrl"] ?? baseAddress;   // <-- single line change (no txt file)
if (!apiBaseUrl.EndsWith("/")) apiBaseUrl += "/";
Console.WriteLine($"[DEBUG] BaseAddress: {baseAddress}");
Console.WriteLine($"[DEBUG] EffectiveEnv: {effectiveEnv}");
Console.WriteLine($"[DEBUG] ApiBaseUrl: {apiBaseUrl}");

//HTTP Client
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

//LocalStorage goes below
builder.Services.AddBlazoredLocalStorage();
//Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

var host = builder.Build();

Console.WriteLine("Debug before run");

await host.RunAsync();