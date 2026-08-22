namespace IdAnimal.Shared.DTOs;

public class CattleDto
{
    public int Id { get; set; }
    public string? GlobalId { get; set; }
    public string Caravan { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal? Weight { get; set; }
    public string? Origin { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? GDM { get; set; }
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public int ImageCount { get; set; }
    public int VideoCount { get; set; }
    public string? MainImageUrl { get; set; }
    public Dictionary<string, string>? CustomData { get; set; }

    // Campos extendidos para la vista de detalle (carta del animal)
    public DateTime? BirthDate { get; set; }
    public string? SanitaryHistory { get; set; }
    public int? FeedPasturePct { get; set; }
    public int? FeedFeedlotPct { get; set; }
    public int? FeedOrganicPct { get; set; }
    public int? WeightProgressPct { get; set; }
    /// <summary>CSV de identificadores de certificación, ej. "organic,brc,welfare,senasa".</summary>
    public string? Certifications { get; set; }
}
