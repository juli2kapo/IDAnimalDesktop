using System.Net.Http.Json;
using IdAnimal.Shared.DTOs;

namespace IdAnimal.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        // var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";
        var baseUrl = "https://api.idanimal.tech";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        catch
        {
            return null;
        }
    }
}
