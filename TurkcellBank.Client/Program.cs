using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using TurkcellBank.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");

// Load appsettings.json (already served from wwwroot)
var config = new ConfigurationBuilder()
    .AddJsonStream(await new HttpClient().GetStreamAsync("appsettings.json"))
    .Build();

// Get API base URL from config
var apiBaseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001/";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

await builder.Build().RunAsync();