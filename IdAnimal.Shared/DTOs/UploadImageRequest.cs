namespace IdAnimal.Shared.DTOs;

public class UploadImageRequest
{
    public int CattleId { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
    public string ImageType { get; set; } = "Snout"; // Snout, Full, Caravan
    public string? Descriptors { get; set; } // JSON for SIFT descriptors
    public string? Keypoints { get; set; }   // JSON for SIFT keypoints
}
