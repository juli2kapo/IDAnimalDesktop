namespace IdAnimal.Shared.DTOs.Clientes;

/// Una fila del historial. `Origen` es "campo" (la app) o "cliente" (el portal).
public class VerificacionDto
{
    public string Origen { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public bool? IsMatch { get; set; }
    public double? Distance { get; set; }
    public double? Threshold { get; set; }
    public string? Reason { get; set; }
    public string? Outcome { get; set; }
}
