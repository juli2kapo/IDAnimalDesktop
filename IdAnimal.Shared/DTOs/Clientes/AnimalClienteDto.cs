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
}
