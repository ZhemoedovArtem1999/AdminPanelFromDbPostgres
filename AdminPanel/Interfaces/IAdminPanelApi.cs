using AdminPanel.Domain.Models;
using AdminPanel.Models;
using Refit;

namespace AdminPanel.Interfaces;

public interface IAdminPanelApi
{
    [Get("/api/admin/schemas")]
    Task<List<SchemaInfo>> GetSchemasAsync();

    [Get("/api/admin/schemaDescription/{schema}")]
    Task<string> GetSchemaDescriptionAsync(string schema);

    [Get("/api/admin/schemas/{schema}/tables")]
    Task<List<TableInfo>> GetTablesAsync(string schema);

    [Get("/api/admin/tableDescription/{schema}/{table}")]
    Task<string> GetTableDescriptionAsync(string schema, string table);

    [Get("/api/admin/schemas/{schema}/tables/{table}/columns")]
    Task<List<ColumnInfo>> GetColumnsAsync(string schema, string table);

    [Get("/api/admin/schemas/{schema}/tables/{table}/primarykeys")]
    Task<List<string>> GetPrimaryKeysAsync(string schema, string table);

    [Post("/api/admin/schemas/{schema}/tables/{table}/data")]
    Task<List<Dictionary<string, object>>> GetDataAsync(string schema, string table, [Body] DataRequest request);

    [Post("/api/admin/schemas/{schema}/tables/{table}/count")]
    Task<int> GetCountAsync(string schema, string table, [Body] List<SearchCondition> filters);

    [Post("/api/admin/schemas/{schema}/tables/{table}/insert")]
    Task InsertAsync(string schema, string table, [Body] Dictionary<string, object> values);

    [Put("/api/admin/schemas/{schema}/tables/{table}/update")]
    Task UpdateAsync(string schema, string table, [Body] UpdateRequest request);

    [Delete("/api/admin/schemas/{schema}/tables/{table}/delete")]
    Task DeleteAsync(string schema, string table, [Body] Dictionary<string, object> keyValues);
}
