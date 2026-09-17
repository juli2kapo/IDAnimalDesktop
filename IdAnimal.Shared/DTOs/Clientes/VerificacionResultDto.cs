namespace IdAnimal.Shared.DTOs.Clientes;

public class VerificacionResultDto
{
    public string Outcome { get; set; } = string.Empty;
    public bool? IsMatch { get; set; }
    public double? Distance { get; set; }
    public double? Threshold { get; set; }
    public double? PGood { get; set; }          // -> p_good
    public string? Reason { get; set; }
    public string? Caravan { get; set; }
    public string? VerificadoEn { get; set; }   // -> verificado_en
}
