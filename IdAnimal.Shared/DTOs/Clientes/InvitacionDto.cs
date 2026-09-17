namespace IdAnimal.Shared.DTOs.Clientes;

public class InvitacionDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string ExpiresAt { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? InvitadoPor { get; set; }
}
