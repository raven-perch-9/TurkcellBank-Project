using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TurkcellBank.Client;
using Blazored.LocalStorage;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
//MudBlazor templates will be used.
builder.Services.AddMudServices();
//HTTP Client goes below
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7104/") });
//LocalStorage goes below
builder.Services.AddBlazoredLocalStorage();

// This is used to load appsettings.json (already served from wwwroot).
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = new ConfigurationBuilder()
    .AddJsonStream(await http.GetStreamAsync("appsettings.json"))
    .Build();

// We get API base URL from config via this part.
var apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001/";

await builder.Build().RunAsync();