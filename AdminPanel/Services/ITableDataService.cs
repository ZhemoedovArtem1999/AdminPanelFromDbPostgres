using AdminPanel.Models;

namespace AdminPanel.Services;

public interface ITableDataService
{
    Task<List<Dictionary<string, object>>> GetDataAsync(string schema, string table, string orderByColumn, int page = 1, int pageSize = 50);
    Task<int> GetCountAsync(string schema, string table);
    Task<List<Dictionary<string, object>>> GetDataAsync(string schema, string table, string orderByColumn, List<SearchCondition> filters, List<ColumnInfo> columns, int page = 1, int pageSize = 50);
    Task<int> GetCountAsync(string schema, string table, List<SearchCondition> filters, List<ColumnInfo> columns);
    Task InsertAsync(string schema, string table, Dictionary<string, object> values);
    Task UpdateAsync(string schema, string table, Dictionary<string, object> values, List<string> keyColumns, Dictionary<string, object> keyValues);
    Task DeleteAsync(string schema, string table, Dictionary<string, object> keyValues);
}
