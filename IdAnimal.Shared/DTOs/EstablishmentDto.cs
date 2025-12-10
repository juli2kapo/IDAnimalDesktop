namespace IdAnimal.Shared.DTOs;

public class EstablishmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RegisterDate { get; set; }
    public DateTime EstablishmentRegisterDate { get; set; }
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Renspa { get; set; } = string.Empty;
    public int CattleCount { get; set; }
}
