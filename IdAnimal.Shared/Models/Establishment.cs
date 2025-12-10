namespace IdAnimal.Shared.Models;

public class Establishment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RegisterDate { get; set; }
    public DateTime EstablishmentRegisterDate { get; set; }
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Renspa { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Cattle> Cattle { get; set; } = new List<Cattle>();
}
