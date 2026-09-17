namespace IdAnimal.Shared.DTOs.Clientes;

public class AnimalesPageDto
{
    public List<AnimalClienteDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
