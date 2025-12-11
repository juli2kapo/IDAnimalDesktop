using IdAnimal.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms; 
using System.Net.Http.Headers;               
using System.Net.Http;                      

namespace IdAnimal.Web.Services;

public class CattleService
{
    private readonly ApiClient _apiClient;

    public CattleService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<CattleDto>?> GetAllAsync(int? establishmentId = null)
    {
        var endpoint = establishmentId.HasValue
            ? $"/api/cattle?establishmentId={establishmentId.Value}"
            : "/api/cattle";

        return await _apiClient.GetAsync<List<CattleDto>>(endpoint);
    }

    public async Task<CattleDetailDto?> GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync<CattleDetailDto>($"/api/cattle/{id}");
    }

    public async Task<bool> CreateAsync(CattleDto dto)
    {
        var response = await _apiClient.PostAsync("/api/cattle", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, CattleDto dto)
    {
        var response = await _apiClient.PutAsync($"/api/cattle/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"/api/cattle/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadImageAsync(int cattleId, IBrowserFile file, string imageType)
{
    long maxFileSize = 1024 * 1024 * 5; // 5MB match your Razor const

    try 
    {
        using var content = new MultipartFormDataContent();

        // 1. Add form fields (Must match [FromForm] names in Controller)
        content.Add(new StringContent(cattleId.ToString()), "cattleId");
        content.Add(new StringContent(imageType), "imageType");

        // 2. Add the file stream
        // We use StreamContent to avoid loading the whole file into RAM (Base64)
        var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        // "file" is the name of the parameter in your Controller: UploadImage(..., IFormFile file)
        content.Add(fileContent, "file", file.Name);

        // NOTE: I am assuming your _apiClient wraps HttpClient. 
        // If ApiClient.PostAsync only takes objects for JSON, you should use the raw HttpClient here.
        // Assuming _apiClient has a public HttpClient or you inject HttpClient directly:
        
        // Example if using direct HttpClient:
        // var response = await _httpClient.PostAsync("/api/cattle/upload-image", content);
        
        // Adapting to your existing _apiClient pattern (assuming it can handle HttpContent or you add this method):
        var response = await _apiClient.PostContentAsync("/api/cattle/upload-image", content);
        var text = await response.Content.ReadAsStringAsync();
        Console.WriteLine(text);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Service Upload Error: {ex.Message}");
        return false;
    }
}
}
