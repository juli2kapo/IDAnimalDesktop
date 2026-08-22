using System.Net.Http.Json;

namespace IdAnimal.Web.Services;

/// <summary>
/// Ficha pública de un animal (la que abre el QR). No usa <see cref="ApiClient"/>
/// a propósito: ese adjunta el JWT y lo lee de ProtectedLocalStorage, que no está
/// disponible durante el prerender ni para un visitante sin sesión.
/// </summary>
public class PublicAnimalService
{
    private readonly HttpClient _http;

    public PublicAnimalService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(ApiUrl.BaseUrl);
    }

    public async Task<PublicAnimalDto?> GetByGlobalIdAsync(string globalId)
    {
        try
        {
            var resp = await _http.GetAsync($"/api/v1/publico/animal/{globalId}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<PublicAnimalDto>(ApiJson.Options);
        }
        catch (Exception)
        {
            // Un animal inexistente y un backend caído se ven igual desde acá:
            // la página muestra "no encontrado" en ambos casos.
            return null;
        }
    }
}

public class PublicAnimalDto
{
    public string GlobalId { get; set; } = string.Empty;
    public string Caravan { get; set; } = string.Empty;
    public string? Eid { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? EstablishmentName { get; set; }
    public string? ImageUrl { get; set; }
    public int ImageCount { get; set; }
    public Dictionary<string, string> CustomData { get; set; } = new();
}
