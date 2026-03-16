using AdminPanel.Models;

namespace AdminPanel.Services;

public interface IDatabaseSchemaService
{
    Task<List<SchemaInfo>> GetSchemasAsync();
    Task<string?> GetSchemaDescriptionAsync(string schema);
    Task<List<TableInfo>> GetTablesAsync(string schema);
    Task<string?> GetTableDescriptionAsync(string schema, string table);
    Task<List<ColumnInfo>> GetColumnsAsync(string schema, string table);
    Task<List<string>> GetPrimaryKeyColumnsAsync(string schema, string table);
}
