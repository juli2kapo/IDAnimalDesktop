using IdAnimal.Shared.DTOs;

namespace IdAnimal.Web.Services;

public class CustomDataService
{
    private readonly ApiClient _apiClient;

    public CustomDataService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<CustomDataColumnDto>?> GetAllColumnsAsync()
    {
        return await _apiClient.GetAsync<List<CustomDataColumnDto>>("/api/v1/datos-custom");
    }

    public async Task<CustomDataColumnDto?> GetColumnByIdAsync(int id)
    {
        return await _apiClient.GetAsync<CustomDataColumnDto>($"/api/v1/datos-custom/{id}");
    }

    public async Task<bool> CreateColumnAsync(CustomDataColumnDto column)
    {
        var response = await _apiClient.PostAsync("/api/v1/datos-custom", column);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateColumnAsync(int id, CustomDataColumnDto column)
    {
        var response = await _apiClient.PutAsync($"/api/v1/datos-custom/{id}", column);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteColumnAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"/api/v1/datos-custom/{id}");
        return response.IsSuccessStatusCode;
    }
}
