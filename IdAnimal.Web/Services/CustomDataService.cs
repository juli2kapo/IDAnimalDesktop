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
        return await _apiClient.GetAsync<List<CustomDataColumnDto>>("/api/customdatacolumns");
    }

    public async Task<CustomDataColumnDto?> GetColumnByIdAsync(int id)
    {
        return await _apiClient.GetAsync<CustomDataColumnDto>($"/api/customdatacolumns/{id}");
    }

    public async Task<bool> CreateColumnAsync(CustomDataColumnDto column)
    {
        var response = await _apiClient.PostAsync("/api/customdatacolumns", column);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateColumnAsync(int id, CustomDataColumnDto column)
    {
        var response = await _apiClient.PutAsync($"/api/customdatacolumns/{id}", column);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteColumnAsync(int id)
    {
        var response = await _apiClient.DeleteAsync($"/api/customdatacolumns/{id}");
        return response.IsSuccessStatusCode;
    }
}
