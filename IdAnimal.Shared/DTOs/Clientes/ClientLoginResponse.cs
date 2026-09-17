namespace IdAnimal.Shared.DTOs.Clientes;

/// Respuesta de POST /api/v1/clientes/auth/login.
/// Los nombres calzan con el snake_case del backend vía ApiJson.Options.
public class ClientLoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
}
