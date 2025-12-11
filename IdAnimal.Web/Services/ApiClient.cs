using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IdAnimal.Web.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AuthStateProvider _authStateProvider;

    public ApiClient(HttpClient httpClient, IConfiguration configuration, AuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authStateProvider = authStateProvider;

        //var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";
        // var baseUrl = "https://localhost:5001";
        var baseUrl = "https://api.idanimal.tech";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.PostAsJsonAsync(endpoint, data);
    }

    public async Task<HttpResponseMessage> PostContentAsync(string endpoint, HttpContent content)
    {
        await SetAuthHeaderAsync();
        // We use PostAsync here directly so the 'content' can define its own 
        // Content-Type (e.g., multipart/form-data)
        return await _httpClient.PostAsync(endpoint, content);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.PutAsJsonAsync(endpoint, data);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.DeleteAsync(endpoint);
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
}
