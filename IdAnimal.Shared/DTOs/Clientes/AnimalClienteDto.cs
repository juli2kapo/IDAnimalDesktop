namespace IdAnimal.Shared.DTOs.Clientes;

public class AnimalClienteDto
{
    public int Id { get; set; }
    public string GlobalId { get; set; } = string.Empty;
    public string Caravan { get; set; } = string.Empty;
    public string? Eid { get; set; }
    public string? Name { get; set; }
    public double? Weight { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public int UserId { get; set; }
    public int EstablishmentId { get; set; }
    public string? Establecimiento { get; set; }
    public string? Provincia { get; set; }
    public string? Productor { get; set; }
    public string? Renspa { get; set; }
    public List<VerificacionDto>? Verificaciones { get; set; }

    /// Primera imagen no excluida del animal, tal como la devuelve el LISTADO
    /// (`main_image_url`). Puede ser null: hay animales sin ninguna foto
    /// cargada, y la fila tiene que seguir mostrandose igual.
    /// SnakeCaseLower convierte "MainImageUrl" -> "main_image_url" (verificado
    /// ejecutando la policy), asi que no hace falta [JsonPropertyName].
    public string? MainImageUrl { get; set; }

    /// Album completo, solo en la FICHA (`imagenes`). En el listado viene null.
    public List<ImagenAnimalDto>? Imagenes { get; set; }

    /// Cantidad de imagenes de la ficha (`image_count`).
    public int ImageCount { get; set; }
}

/// Una imagen del album. El backend devuelve las filas crudas de
/// `imagenes_animal`, con `image_url` y `added_at`.
public class ImagenAnimalDto
{
    public string? ImageUrl { get; set; }
    public string? AddedAt { get; set; }
}
