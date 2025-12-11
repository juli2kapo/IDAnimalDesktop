namespace IdAnimal.Shared.DTOs;

public class CattleDto
{
    public int Id { get; set; }
    public Guid GlobalId { get; set; } = Guid.NewGuid();
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
}
