namespace IdAnimal.API.Services;

public interface ICloudStorageService
{
    Task<string> UploadImageAsync(string base64Image, string folder, string fileName);
    Task<bool> DeleteImageAsync(string publicId);
}
