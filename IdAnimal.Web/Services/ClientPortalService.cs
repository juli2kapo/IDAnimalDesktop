using System.Net.Http.Json;
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

    /// Consulta pública de una invitación. Sin JWT a propósito: el invitado
    /// todavía no tiene cuenta. Ver PublicAnimalService, mismo motivo.
    public async Task<InvitacionPublicaDto?> GetInvitacionAsync(string token)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(ApiUrl.BaseUrl) };
            var r = await http.GetAsync($"/api/v1/clientes/invitaciones/{token}");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadFromJsonAsync<InvitacionPublicaDto>(ApiJson.Options);
        }
        catch { return null; }
    }

    public async Task<InvitacionCanjeDto?> CanjearInvitacionAsync(
        string token, string fullName, string password)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(ApiUrl.BaseUrl) };
            var r = await http.PostAsJsonAsync(
                $"/api/v1/clientes/invitaciones/{token}",
                new { full_name = fullName, password }, ApiJson.Options);
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadFromJsonAsync<InvitacionCanjeDto>(ApiJson.Options);
        }
        catch { return null; }
    }

    /// Listado de invitaciones pendientes del cliente. Nunca trae el token/url.
    public async Task<List<InvitacionDto>?> GetInvitacionesAsync()
        => await _apiClient.GetAsync<List<InvitacionDto>>("/api/v1/clientes/invitaciones");

    /// La URL de la invitación viene UNA sola vez, en esta respuesta.
    public async Task<InvitacionCreadaDto?> InvitarAsync(string email, bool esAdmin)
    {
        var r = await _apiClient.PostAsync("/api/v1/clientes/invitaciones",
                                           new { email, is_admin = esAdmin });
        if (!r.IsSuccessStatusCode) return null;
        return await r.Content.ReadFromJsonAsync<InvitacionCreadaDto>(ApiJson.Options);
    }

    public async Task<bool> RevocarInvitacionAsync(int id)
    {
        var r = await _apiClient.DeleteAsync($"/api/v1/clientes/invitaciones/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Campos custom (columnas) ─────────────────────────────────────────

    public async Task<List<ColumnaClienteDto>?> GetColumnasAsync()
        => await _apiClient.GetAsync<List<ColumnaClienteDto>>("/api/v1/clientes/columnas");

    public async Task<bool> CrearColumnaAsync(string nombre)
        => (await _apiClient.PostAsync("/api/v1/clientes/columnas",
                new { column_name = nombre })).IsSuccessStatusCode;

    public async Task<bool> BorrarColumnaAsync(int id)
        => (await _apiClient.DeleteAsync($"/api/v1/clientes/columnas/{id}")).IsSuccessStatusCode;

    // ── Usuarios y roles ──────────────────────────────────────────────────

    public async Task<List<UsuarioClienteDto>?> GetUsuariosAsync()
        => await _apiClient.GetAsync<List<UsuarioClienteDto>>("/api/v1/clientes/usuarios");

    public async Task<bool> SetRolesAsync(int userId, bool admin, bool verify, bool annotate)
        => (await _apiClient.PutAsync($"/api/v1/clientes/usuarios/{userId}/roles",
                new { is_client_admin = admin, can_verify = verify,
                      can_annotate = annotate })).IsSuccessStatusCode;
}
