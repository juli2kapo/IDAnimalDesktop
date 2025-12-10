using IdAnimal.Shared.DTOs;

namespace IdAnimal.Web.Services;

public class EstablishmentService
{
    private readonly ApiClient _apiClient;

    public EstablishmentService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<EstablishmentDto>?> GetAllAsync()
    {
        return await _apiClient.GetAsync<List<EstablishmentDto>>("/api/establishments");
    }

    public async Task<EstablishmentDto?> GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync<EstablishmentDto>($"/api/establishments/{id}");
    }

    public async Task<bool> CreateAsync(EstablishmentDto dto)
    {
        var response = await _apiClient.PostAsync("/api/establishments", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, EstablishmentDto dto)
    {
        var response = await _apiClient.PutAsync($"/api/establishments/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"/api/establishments/{id}");
        return response.IsSuccessStatusCode;
    }
}
