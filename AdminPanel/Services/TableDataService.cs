using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.Text;

namespace AdminPanel.Services;

public class TableDataService : ITableDataService
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

    public async Task<List<Dictionary<string, object>>> GetDataAsync(
        string schema,
        string table,
        string orderByColumn,
        List<SearchCondition> filters,
        List<ColumnInfo> columns,
        int page = 1,
        int pageSize = 50)
    {
        var sql = new StringBuilder($"SELECT * FROM \"{schema}\".\"{table}\"");
        var parameters = new Dictionary<string, object>();
        var whereClauses = new List<string>();

        if (filters != null && filters.Any())
        {
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
                if (column == null) continue;

                string paramName = $"@p{i}";
                string clause = BuildWhereClause(column, filter.Operator, filter.Value, paramName, out object paramValue);
                if (clause != null)
                {
                    whereClauses.Add(clause);
                    parameters[paramName] = paramValue;
                }
            }
        }

        if (whereClauses.Any())
        {
            sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
        }

        sql.Append($" ORDER BY \"{orderByColumn}\"");
        sql.Append(" LIMIT @limit OFFSET @offset");
        parameters["limit"] = pageSize;
        parameters["offset"] = (page - 1) * pageSize;

        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync(sql.ToString(), parameters);
        return rows
            .Select(x => (IDictionary<string, object>)x)
            .Select(dict => dict.ToDictionary(k => k.Key, v => v.Value))
            .ToList();
    }

    public async Task<int> GetCountAsync(
        string schema,
        string table,
        List<SearchCondition> filters,
        List<ColumnInfo> columns)
    {
        var sql = new StringBuilder($"SELECT COUNT(*) FROM \"{schema}\".\"{table}\"");
        var parameters = new Dictionary<string, object>();
        var whereClauses = new List<string>();

        if (filters != null && filters.Any())
        {
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase));
                if (column == null) continue;

                string paramName = $"@p{i}";
                string clause = BuildWhereClause(column, filter.Operator, filter.Value, paramName, out object paramValue);
                if (clause != null)
                {
                    whereClauses.Add(clause);
                    parameters[paramName] = paramValue;
                }
            }
        }

        if (whereClauses.Any())
        {
            sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
    }

    private string BuildWhereClause(ColumnInfo column, string op, string rawValue, string paramName, out object paramValue)
    {
        paramValue = null;
        if (string.IsNullOrWhiteSpace(rawValue)) return null;

        var colType = column.DataType;

        if (IsBooleanType(colType) && op?.ToLower() != "equals")
        {
            return null;
        }

        string sqlOperator = op?.ToLower() switch
        {
            "equals" => "=",
            "gt" => ">",
            "lt" => "<",
            _ => null
        };

        if (op?.ToLower() == "contains" || op?.ToLower() == "startswith")
        {
            if (!IsTextType(colType)) return null;
            paramValue = op.ToLower() == "contains" ? $"%{rawValue}%" : $"{rawValue}%";
            return $"\"{column.ColumnName}\" ILIKE {paramName}";
        }

        if (sqlOperator == null) return null;

        paramValue = ConvertValue(colType, rawValue);
        return $"\"{column.ColumnName}\" {sqlOperator} {paramName}";
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

    private bool IsTextType(string type) =>
      new[] { "string", "text", "varchar", "char" }.Contains(type?.ToLower());

    private bool IsBooleanType(string type)
    {
        if (string.IsNullOrEmpty(type)) return false;
        var lower = type.ToLower();
        return lower.Contains("boolean");
    }
    private object ConvertValue(string type, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DBNull.Value;
        var lowerType = type.ToLowerInvariant();

        try
        {
            if (lowerType.Contains("bool"))
            {
                return value.ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "да" or "t" => true,
                    "false" or "0" or "no" or "нет" or "f" => false,
                    _ => bool.Parse(value)
                };
            }

            if (lowerType.Contains("smallint")) return short.Parse(value, CultureInfo.InvariantCulture);
            if (lowerType.Contains("int") && !lowerType.Contains("bigint"))
                return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (lowerType.Contains("bigint"))
                return long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

            if (lowerType.Contains("decimal") || lowerType.Contains("numeric"))
                return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            if (lowerType.Contains("real") || lowerType.Contains("float4"))
                return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            if (lowerType.Contains("double") || lowerType.Contains("float8"))
                return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

            if (lowerType.Contains("date") && !lowerType.Contains("timestamp"))
            {
                if (DateOnly.TryParse(value, out var dateOnly))
                    return dateOnly.ToDateTime(TimeOnly.MinValue);
                return DateTime.Parse(value, CultureInfo.CurrentCulture).Date;
            }
            if (lowerType.Contains("timestamp"))
            {
                return DateTime.Parse(value, CultureInfo.CurrentCulture);
            }
            if (lowerType.Contains("time") && !lowerType.Contains("timestamp"))
            {
                if (TimeOnly.TryParse(value, out var timeOnly))
                    return timeOnly.ToTimeSpan();
                return TimeSpan.Parse(value, CultureInfo.CurrentCulture);
            }

            if (lowerType.Contains("uuid"))
                return Guid.Parse(value);

            if (lowerType.Contains("bool"))
            {
                return value.ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "да" or "t" => true,
                    "false" or "0" or "no" or "нет" or "f" => false,
                    _ => bool.Parse(value)
                };
            }

            return value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot convert '{value}' to type {type}", ex);
        }
    }
}
