namespace IdAnimal.Shared.DTOs.Clientes;

/// Los flags vienen como `int` (0/1), no `bool`: SQLite los guarda así y el
/// backend los devuelve tal cual. Mapearlos a `bool` los dejaría en `false`
/// siempre, en silencio.
public class UsuarioClienteDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;     // full_name
    public int IsActive { get; set; }                        // is_active
    public int IsClientAdmin { get; set; }                   // is_client_admin
    public int CanVerify { get; set; }                       // can_verify
    public int CanAnnotate { get; set; }                     // can_annotate
    public string? LastLoginAt { get; set; }                 // last_login_at
}
