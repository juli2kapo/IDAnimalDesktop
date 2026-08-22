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
            ? $"/api/v1/ganado?establishment_id={establishmentId.Value}"
            : "/api/v1/ganado";

        return await _apiClient.GetAsync<List<CattleDto>>(endpoint);
    }

    public async Task<CattleDetailDto?> GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync<CattleDetailDto>($"/api/v1/ganado/{id}");
    }

    public async Task<bool> CreateAsync(CattleDto dto)
    {
        // Backend Python exige dict (no null) para custom_data.
        dto.CustomData ??= new Dictionary<string, string>();
        var response = await _apiClient.PostAsync("/api/v1/ganado", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, CattleDto dto)
    {
        dto.CustomData ??= new Dictionary<string, string>();
        var response = await _apiClient.PutAsync($"/api/v1/ganado/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"/api/v1/ganado/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadImageAsync(string? cattleId, IBrowserFile file, string imageType)
    {
        long maxFileSize = 1024 * 1024 * 5; // 5MB

        try
        {
            using var content = new MultipartFormDataContent();

            // El backend Python espera nombres en snake_case (ver app/routers/ganado.py).
            content.Add(new StringContent(cattleId ?? ""), "cattle_global_id");
            content.Add(new StringContent(imageType), "image_type");

            var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _apiClient.PostContentAsync(
                "/api/v1/ganado/upload-imagen", content);
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
