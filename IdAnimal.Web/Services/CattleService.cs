using IdAnimal.Shared.DTOs;

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

    public async Task<bool> UploadImageAsync(UploadImageRequest request)
    {
        var response = await _apiClient.PostAsync("/api/cattle/upload-image", request);
        return response.IsSuccessStatusCode;
    }
}
