namespace IdAnimal.Shared.Models;

public class CustomDataColumn
{
    public int Id { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = "String"; // String, Number, Date, Boolean
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<CustomDataValue> Values { get; set; } = new List<CustomDataValue>();
}
