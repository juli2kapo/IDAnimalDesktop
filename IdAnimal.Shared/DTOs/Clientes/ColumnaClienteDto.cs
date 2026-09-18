namespace IdAnimal.Shared.DTOs.Clientes;

public class ColumnaClienteDto
{
    public int Id { get; set; }
    public string ColumnName { get; set; } = string.Empty;   // column_name
    public string DataType { get; set; } = "String";         // data_type
    public string? CreatedAt { get; set; }                   // created_at
}
