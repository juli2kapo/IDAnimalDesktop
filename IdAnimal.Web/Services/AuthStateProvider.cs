using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IdAnimal.Web.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly HttpClient _httpClient;
    private AuthenticationState? _cachedState; // 1. Memory Cache

    private const string TokenKey = "authToken";

    public AuthStateProvider(ProtectedLocalStorage localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 2. Return cached state if we have it (Super fast, no JS Interop needed)
        if (_cachedState != null)
        {
            return _cachedState;
        }

        try
        {
            // 3. Only read from storage if we don't have it in memory yet (e.g. on Page Refresh)
            var tokenResult = await _localStorage.GetAsync<string>(TokenKey);

            if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
            {
                var token = tokenResult.Value;
                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                var user = new ClaimsPrincipal(identity);
                
                // Set the auth header for future requests
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _cachedState = new AuthenticationState(user);
                return _cachedState;
            }
        }
        catch
        {
            // If LocalStorage fails (e.g. during pre-rendering), return Anonymous
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task LoginAsync(string token)
    {
        // 4. Update Memory IMMEDIATELY
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        _cachedState = new AuthenticationState(user);

        // 5. Notify the UI instantly (doesn't wait for disk write)
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));

        // 6. Set header and save to storage in background
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _localStorage.SetAsync(TokenKey, token);
    }

    public async Task LogoutAsync()
    {
        _cachedState = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await _localStorage.DeleteAsync(TokenKey);

        var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
    }

    // Helper to extract data directly from the Token
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}