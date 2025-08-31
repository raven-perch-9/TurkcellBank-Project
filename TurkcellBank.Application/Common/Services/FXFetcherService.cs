using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.Common.DTOs;
using System.Net.Http.Json;

namespace TurkcellBank.Application.Common.Services
{
    public sealed class FXFetcherService : IFXFetcherService
    {
        private readonly HttpClient _http = new();
        public FXFetcherService(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri("https://open.er-api.com/v6/"); // safety net
        }

        public async Task<FXRatesDTO> GetLatestAsync(string @base, string[] symbols)
        {
            var baseCur = (@base ?? "USD").Trim().ToUpperInvariant();
            using var resp = await _http.GetAsync($"latest/{baseCur}");
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<ExHostLatest>();
            if (json == null || json.rates == null) throw new InvalidOperationException("No FX data found.");

            return new FXRatesDTO(
                json.@base.ToUpperInvariant(),
                DateTimeOffset.UtcNow,
                json.rates
                );
        }

        private sealed class ExHostLatest
        {
            public string @base { get; set; } = "EUR";
            public Dictionary<string, decimal> rates { get; set; } = new();
        }
    }
}
