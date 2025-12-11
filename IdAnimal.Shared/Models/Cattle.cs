namespace IdAnimal.Shared.Models;

public class Cattle
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Caravan { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal? Weight { get; set; }
    public string? Origin { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? GDM { get; set; }
    public int EstablishmentId { get; set; }
    public Establishment? Establishment { get; set; }
    public ICollection<CattleImage> Images { get; set; } = new List<CattleImage>();
    public ICollection<CattleFullImage> FullImages { get; set; } = new List<CattleFullImage>();
    public ICollection<CattleVideo> Videos { get; set; } = new List<CattleVideo>();
    public ICollection<CustomDataValue> CustomDataValues { get; set; } = new List<CustomDataValue>();
}
