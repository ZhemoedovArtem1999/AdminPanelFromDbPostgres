using Dapper;
using Npgsql;

namespace AdminPanel.WebApi.Services;

public class TableDataService
{
    private readonly string _connectionString;

    public TableDataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Dictionary<string, object>>> GetDataAsync(
        string schema,
        string table,
        string orderByColumn, 
        int page = 1,
        int pageSize = 50)
    {
        var offset = (page - 1) * pageSize;
        var sql = $@"
            SELECT * 
            FROM ""{schema}"".""{table}"" 
            ORDER BY ""{orderByColumn}"" 
            LIMIT @pageSize 
            OFFSET @offset;";

        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync(sql, new { pageSize, offset });
        return rows
            .Select(x => (IDictionary<string, object>)x)
            .Select(dict => dict.ToDictionary(k => k.Key, v => v.Value))
            .ToList();
    }

    public async Task<int> GetCountAsync(string schema, string table)
    {
        var sql = $"SELECT COUNT(*) FROM \"{schema}\".\"{table}\";";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task InsertAsync(string schema, string table, Dictionary<string, object> values)
    {
        var columns = string.Join(", ", values.Keys.Select(k => $"\"{k}\""));
        var parameters = string.Join(", ", values.Keys.Select(k => "@" + k));
        var sql = $"INSERT INTO \"{schema}\".\"{table}\" ({columns}) VALUES ({parameters});";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, values);
    }

    public async Task UpdateAsync(
        string schema, string table,
        Dictionary<string, object> values,
        List<string> keyColumns,
        Dictionary<string, object> keyValues)
    {
        var setClause = string.Join(", ",
            values.Keys.Where(k => !keyColumns.Contains(k))
                       .Select(k => $"\"{k}\" = @{k}"));
        var whereClause = string.Join(" AND ",
            keyColumns.Select(k => $"\"{k}\" = @{k}_key"));
        var sql = $"UPDATE \"{schema}\".\"{table}\" SET {setClause} WHERE {whereClause};";

        var parameters = new Dictionary<string, object>(values);
        foreach (var kv in keyValues)
        {
            parameters[$"{kv.Key}_key"] = kv.Value;
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task DeleteAsync(
        string schema, string table,
        Dictionary<string, object> keyValues)
    {
        var whereClause = string.Join(" AND ",
            keyValues.Keys.Select(k => $"\"{k}\" = @{k}"));
        var sql = $"DELETE FROM \"{schema}\".\"{table}\" WHERE {whereClause};";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, keyValues);
    }
}
