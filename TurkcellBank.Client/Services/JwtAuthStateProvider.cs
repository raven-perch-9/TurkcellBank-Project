using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace TurkcellBank.Client.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;

        public JwtAuthStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _js.InvokeAsync<string>("sessionStorage.getItem", "authToken");
                if (string.IsNullOrWhiteSpace(token))
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

                var payload = DecodeJwtPayload(token);

                // Expiration check
                if (payload.TryGetValue("exp", out var expObj) &&
                    long.TryParse(expObj.ToString(), out long expUnix))
                {
                    var exp = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                    if (exp < DateTime.UtcNow)
                    {
                        await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                }

                var claims = payload.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                var identity = new ClaimsIdentity(claims, "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        // This now accepts raw API response (JSON or token)
        public async Task MarkUserAsAuthenticated(string responseContent)
        {
            string token;
            try
            {
                // Try to parse JSON { "token": "..." }
                using var doc = JsonDocument.Parse(responseContent);
                token = doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
            }
            catch
            {
                // Fallback: treat it as raw token
                token = responseContent;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                await _js.InvokeVoidAsync("sessionStorage.setItem", "authToken", token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        // Helper: Decode JWT payload
        private static Dictionary<string, object> DecodeJwtPayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                throw new Exception("Invalid JWT format");

            var payload = parts[1];
            var json = System.Text.Encoding.UTF8.GetString(PadBase64(payload));
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
        }

        private static byte[] PadBase64(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
        }
    }
}