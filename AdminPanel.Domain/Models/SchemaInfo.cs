namespace AdminPanel.Domain.Models;

public class SchemaInfo
{
    public string SchemaName { get; set; }
    public string Description { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(Description)
           ? SchemaName
           : Description;
}
