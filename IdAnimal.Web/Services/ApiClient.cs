using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace IdAnimal.Web.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AuthStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;

    public ApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        AuthStateProvider authStateProvider,
        NavigationManager navigation)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authStateProvider = authStateProvider;
        _navigation = navigation;

        var baseUrl = "https://api.idanimal.tech";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await HandleUnauthorizedAsync();
            return default;
        }

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(ApiJson.Options);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, ApiJson.Options);
        await HandleIfUnauthorizedAsync(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostContentAsync(string endpoint, HttpContent content)
    {
        await SetAuthHeaderAsync();
        // PostAsync directo: 'content' define su propio Content-Type (e.g., multipart/form-data)
        var response = await _httpClient.PostAsync(endpoint, content);
        await HandleIfUnauthorizedAsync(response);
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, ApiJson.Options);
        await HandleIfUnauthorizedAsync(response);
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint);
        await HandleIfUnauthorizedAsync(response);
        return response;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authStateProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Un 401 significa que la sesión ya terminó (token vencido, revocado, etc.):
    /// la limpiamos y mandamos al login. Un 403 es DISTINTO — autenticado pero sin
    /// permiso (p.ej. un usuario de aseguradora de solo lectura pegándole a un
    /// endpoint de anotar) — eso lo maneja la UI, y deslogear ahí sería un bug nuevo.
    /// </summary>
    private Task HandleIfUnauthorizedAsync(HttpResponseMessage response)
        => response.StatusCode == HttpStatusCode.Unauthorized
            ? HandleUnauthorizedAsync()
            : Task.CompletedTask;

    private async Task HandleUnauthorizedAsync()
    {
        // `LogoutAsync` borra el token de ProtectedLocalStorage, que es JS
        // interop. Durante el PRERENDER estático todavía no hay circuito, y el
        // interop tira `InvalidOperationException` → la página entera muere con
        // un 500. Pasaba justo en el caso más común: una request sin sesión que
        // recibe 401 mientras se prerenderiza.
        //
        // Ahí no hay nada que limpiar (nunca hubo sesión) ni a dónde navegar:
        // se ignora y el circuito interactivo resolverá el estado al conectar.
        try
        {
            await _authStateProvider.LogoutAsync();

            // Evitar loop de redirección si ya estamos en /login.
            var relativePath = _navigation.ToBaseRelativePath(_navigation.Uri);
            if (!relativePath.StartsWith("login", StringComparison.OrdinalIgnoreCase))
            {
                _navigation.NavigateTo("/login", forceLoad: false);
            }
        }
        catch (InvalidOperationException)
        {
            // Prerender sin circuito: no se puede tocar el storage ni navegar.
        }
    }
}
