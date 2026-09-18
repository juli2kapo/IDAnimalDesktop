namespace IdAnimal.Shared.DTOs.Clientes;

public class DashboardDto
{
    public int Productores { get; set; }
    public int Animales { get; set; }

    // Nullable: el backend devuelve null cuando ningún animal tiene peso cargado.
    public double? PesoPromedio { get; set; }
    public double? PesoTotal { get; set; }
    public int AnimalesConPeso { get; set; }
}
