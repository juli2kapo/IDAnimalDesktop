namespace IdAnimal.Shared.DTOs.Clientes;

/// Respuesta de canjear: el usuario queda creado y logueado.
public class InvitacionCanjeDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
