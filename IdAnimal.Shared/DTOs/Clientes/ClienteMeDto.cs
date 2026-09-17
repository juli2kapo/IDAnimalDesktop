namespace IdAnimal.Shared.DTOs.Clientes;

public class ClienteMeDto
{
    public ClienteInfo? Cliente { get; set; }
    public UsuarioInfo? Usuario { get; set; }

    public class ClienteInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Cuit { get; set; }
    }

    public class UsuarioInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
