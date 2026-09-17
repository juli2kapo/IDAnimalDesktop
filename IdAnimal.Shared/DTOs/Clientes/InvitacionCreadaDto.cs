namespace IdAnimal.Shared.DTOs.Clientes;

/// La URL viene UNA sola vez, al crear. Después no se puede recuperar.
public class InvitacionCreadaDto
{
    public string Email { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
