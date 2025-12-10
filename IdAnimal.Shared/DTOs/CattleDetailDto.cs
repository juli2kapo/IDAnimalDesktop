using IdAnimal.Shared.Models;

namespace IdAnimal.Shared.DTOs;

public class CattleDetailDto
{
    public int Id { get; set; }
    public string Caravan { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal? Weight { get; set; }
    public string? Origin { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? GDM { get; set; }
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public List<CattleImageDto> Images { get; set; } = new();
    public List<CattleFullImageDto> FullImages { get; set; } = new();
    public List<CattleVideoDto> Videos { get; set; } = new();
    public Dictionary<string, string>? CustomData { get; set; }
}

public class CattleImageDto
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
}

public class CattleFullImageDto
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
}

public class CattleVideoDto
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
}
