using System.Net.Http.Json;
using IdAnimal.Shared.DTOs;
using IdAnimal.Shared.DTOs.Clientes;

namespace IdAnimal.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        var baseUrl = "https://api.idanimal.tech";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    /// Prueba la puerta de productor y, si rechaza, la de cliente.
    ///
    /// El usuario no elige "soy cliente": escribe su email y cae donde
    /// corresponde. El claim `typ` del token que vuelve decide el nav.
    ///
    /// Un 401/403 de la primera puerta NO es un error a mostrar: es la señal
    /// de que hay que probar la otra. Sólo si las dos rechazan se informa.
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var r = await _httpClient.PostAsJsonAsync(
                "/api/v1/auth/login", request, ApiJson.Options);
            if (r.IsSuccessStatusCode)
                return await r.Content.ReadFromJsonAsync<LoginResponse>(ApiJson.Options);

            // 401/403 -> puede ser un usuario de cliente. Cualquier otro
            // status (500, timeout) es un problema real: no seguir probando.
            if (r.StatusCode != System.Net.HttpStatusCode.Unauthorized &&
                r.StatusCode != System.Net.HttpStatusCode.Forbidden)
                return null;

            var rc = await _httpClient.PostAsJsonAsync(
                "/api/v1/clientes/auth/login", request, ApiJson.Options);
            if (!rc.IsSuccessStatusCode) return null;

            var cli = await rc.Content.ReadFromJsonAsync<ClientLoginResponse>(ApiJson.Options);
            if (cli is null) return null;

            // Se devuelve el mismo LoginResponse para no tocar a los llamadores:
            // lo que distingue al cliente viaja en el claim `typ` del token.
            return new LoginResponse
            {
                UserId = cli.UserId,
                Email = cli.Email,
                FullName = cli.FullName,
                Token = cli.Token,
                ExpiresAt = DateTime.Parse(cli.ExpiresAt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind),
            };
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
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/auth/register", request, ApiJson.Options);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>(ApiJson.Options);
        }
        catch
        {
            return null;
        }
    }
}
