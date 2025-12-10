namespace IdAnimal.Shared.Models;

public class CattleFullImage
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public int CattleId { get; set; }
    public Cattle? Cattle { get; set; }
}
