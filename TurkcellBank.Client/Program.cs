using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TurkcellBank.Client;
using TurkcellBank.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
//MudBlazor templates will be used.
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

//HTTP Client goes below
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7104/") });
//LocalStorage goes below
builder.Services.AddBlazoredLocalStorage();
//Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
// This is used to load appsettings.json (already served from wwwroot).
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = new ConfigurationBuilder()
    .AddJsonStream(await http.GetStreamAsync("appsettings.json"))
    .Build();

// We get API base URL from config via this part.
var apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001/";

await builder.Build().RunAsync();