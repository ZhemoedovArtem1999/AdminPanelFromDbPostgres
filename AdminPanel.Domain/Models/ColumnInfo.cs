namespace AdminPanel.Domain.Models;

public class ColumnInfo
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public string ColumnDefault { get; set; }
    public int OrdinalPosition { get; set; }
    public string Description { get; set; }  // Новое поле для описания

    public bool IsAutoIncrement =>
        IsIdentity || (ColumnDefault?.Contains("nextval") == true);

    // Для отображения в UI
    public string DisplayName => string.IsNullOrWhiteSpace(Description)
        ? ColumnName
        : Description;
}
