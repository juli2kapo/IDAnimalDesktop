namespace IdAnimal.Shared.Models;

public class CustomDataValue
{
    public int Id { get; set; }
    public int CustomDataColumnId { get; set; }
    public CustomDataColumn? CustomDataColumn { get; set; }
    public int CattleId { get; set; }
    public Cattle? Cattle { get; set; }
    public string Value { get; set; } = string.Empty;
}
