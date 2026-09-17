using IdAnimal.Shared.DTOs.Clientes;

namespace IdAnimal.Web.Services;

/// Portal de clientes institucionales (aseguradoras).
///
/// Todos los endpoints van scopeados por el `client_id` del token: el servicio
/// nunca manda un id de cliente, porque el backend no lo acepta.
public class ClientPortalService
{
    private readonly ApiClient _apiClient;

    public ClientPortalService(ApiClient apiClient) => _apiClient = apiClient;

    public async Task<ClienteMeDto?> GetMeAsync()
        => await _apiClient.GetAsync<ClienteMeDto>("/api/v1/clientes/me");

    public async Task<DashboardDto?> GetDashboardAsync()
        => await _apiClient.GetAsync<DashboardDto>("/api/v1/clientes/dashboard");

    public async Task<List<ProductorDto>?> GetProductoresAsync()
        => await _apiClient.GetAsync<List<ProductorDto>>("/api/v1/clientes/productores");

    public async Task<AnimalesPageDto?> BuscarAnimalesAsync(
        string? q = null, int? productorId = null, int? establecimientoId = null,
        string? provincia = null, int page = 1, int pageSize = 50)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");
        if (productorId is int pid) qs.Add($"productor_id={pid}");
        if (establecimientoId is int eid) qs.Add($"establecimiento_id={eid}");
        if (!string.IsNullOrWhiteSpace(provincia))
            qs.Add($"provincia={Uri.EscapeDataString(provincia)}");
        qs.Add($"page={page}");
        qs.Add($"page_size={pageSize}");
        return await _apiClient.GetAsync<AnimalesPageDto>(
            $"/api/v1/clientes/animales?{string.Join("&", qs)}");
    }

    public async Task<AnimalClienteDto?> GetAnimalAsync(string globalId)
        => await _apiClient.GetAsync<AnimalClienteDto>(
            $"/api/v1/clientes/animales/{globalId}");

    /// Sube una foto del morro y devuelve uno de seis resultados.
    ///
    /// Va por PostContentAsync porque es multipart: ApiJson.Options no aplica
    /// a form data, así que el nombre del campo ("file") va a mano, igual que
    /// en CattleService.UploadImageAsync.
    public async Task<VerificacionResultDto?> VerificarAsync(
        string globalId, Stream foto, string fileName)
    {
        var content = new MultipartFormDataContent();
        var sc = new StreamContent(foto);
        sc.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(sc, "file", fileName);

        var r = await _apiClient.PostContentAsync(
            $"/api/v1/clientes/animales/{globalId}/verificar", content);
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<VerificacionResultDto>(ApiJson.Options);
    }
}
