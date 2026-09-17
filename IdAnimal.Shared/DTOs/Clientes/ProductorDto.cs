namespace IdAnimal.Shared.DTOs.Clientes;

public class ProductorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Establecimientos { get; set; }
    public int Animales { get; set; }
}
