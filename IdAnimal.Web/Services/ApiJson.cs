using System.Text.Json;

namespace IdAnimal.Web.Services;

/// <summary>
/// Opciones de JSON compartidas para hablar con el backend Python (sumato_backend).
/// El backend usa snake_case en todos los campos (establishment_id, main_image_url, etc.).
/// </summary>
internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
    };
}
