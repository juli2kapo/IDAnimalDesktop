using System.Text.Json.Serialization;

namespace IdAnimal.Shared.DTOs.Clientes;

public class DashboardDto
{
    public int Productores { get; set; }
    public int Animales { get; set; }

    // JsonNamingPolicy.SnakeCaseLower.ConvertName("Verificaciones30d") == "verificaciones30d",
    // no "verificaciones_30d" (verificado con un chequeo puntual) — atributo explícito para
    // que calce con el backend.
    [JsonPropertyName("verificaciones_30d")]
    public int Verificaciones30d { get; set; }
}
