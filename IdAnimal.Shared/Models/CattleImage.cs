namespace IdAnimal.Shared.Models;

public class CattleImage
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public int CattleId { get; set; }
    public Cattle? Cattle { get; set; }
    public string? Descriptors { get; set; } // JSON string for SIFT descriptors
    public string? Keypoints { get; set; }    // JSON string for SIFT keypoints
}
