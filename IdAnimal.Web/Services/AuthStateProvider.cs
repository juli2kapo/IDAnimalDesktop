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

    // Tolerancia de reloj: si el cliente está levemente desincronizado, no
    // queremos deslogear a alguien por un token que en realidad sigue vigente.
    private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromSeconds(60);

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = new List<Claim>();

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                // Fix: Map 'name' or 'unique_name' to standard ClaimTypes.Name
                if (kvp.Key == "name" || kvp.Key == "unique_name")
                {
                    claims.Add(new Claim(ClaimTypes.Name, kvp.Value.ToString()));
                }

                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }
        }

        return claims;
    }

    /// <summary>
    /// Evalúa si un token JWT ya expiró, mirando el claim "exp" (epoch seconds).
    /// Deliberadamente defensivo: un token malformado, sin "exp", o con un valor
    /// no parseable NUNCA tira excepción acá — se trata como "no expirado", porque
    /// el backend es la autoridad real y va a devolver 401 si el token es inválido.
    /// Tirar una excepción acá rompería el login entero, que es mucho peor que
    /// dejar pasar una llamada a la API que después falla.
    /// </summary>
    private static bool IsTokenExpired(IEnumerable<Claim> claims)
    {
        try
        {
            var expValue = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (string.IsNullOrEmpty(expValue))
            {
                return false; // No hay forma de saber: no lo tratamos como expirado.
            }

            if (!long.TryParse(expValue, out var expSeconds))
            {
                return false; // Valor no parseable: idem.
            }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            return DateTimeOffset.UtcNow > expiresAt.Add(ClockSkewTolerance);
        }
        catch
        {
            return false; // Nunca dejar que esto tire: ver comentario arriba.
        }
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
                var claims = ParseClaimsFromJwt(token).ToList();

                if (IsTokenExpired(claims))
                {
                    // Token vencido: no armamos una sesión "a medias". Lo limpiamos
                    // y devolvemos anónimo, para que la app se comporte como
                    // deslogueada en vez de mostrar nav/nombre con las llamadas
                    // a la API fallando en silencio.
                    await _localStorage.DeleteAsync(TokenKey);
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    return _cachedState;
                }

                var identity = new ClaimsIdentity(claims, "jwt");
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
        // Mismo chequeo que en GetAuthenticationStateAsync: un token vencido
        // nunca debería poder arrancar una sesión, aunque venga de un login
        // recién hecho (p.ej. reloj del backend desincronizado).
        var loginClaims = ParseClaimsFromJwt(token).ToList();
        if (IsTokenExpired(loginClaims))
        {
            await LogoutAsync();
            return;
        }

        // 4. Update Memory IMMEDIATELY
        var identity = new ClaimsIdentity(loginClaims, "jwt");
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
    // private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    // {
    //     var payload = jwt.Split('.')[1];
    //     var jsonBytes = ParseBase64WithoutPadding(payload);
    //     var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
    //     return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    // }

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