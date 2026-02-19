using AdminPanel.WebApi.Models;
using Dapper;
using Npgsql;

namespace AdminPanel.WebApi.Services;

public class DatabaseSchemaService
{
    private readonly string _connectionString;

    public DatabaseSchemaService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<SchemaInfo>> GetSchemasAsync()
    {
        const string sql = @"
            SELECT 
            n.nspname AS schemaName,
            pg_catalog.obj_description(n.oid, 'pg_namespace') AS description
            FROM pg_catalog.pg_namespace n
            WHERE 
            n.nspname NOT LIKE 'pg_%'
            AND n.nspname NOT IN ('information_schema', 'public')
            ORDER BY n.nspname;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<SchemaInfo>(sql)).ToList();
    }

    public async Task<string?> GetSchemaDescriptionAsync(string schema)
    {
        const string sql = @"
            SELECT 
            pg_catalog.obj_description(n.oid, 'pg_namespace') AS description
            FROM pg_catalog.pg_namespace n
            WHERE n.nspname = @schema
            LIMIT 1
            ;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<string>(sql, new { schema })).FirstOrDefault();
    }

    public async Task<List<TableInfo>> GetTablesAsync(string schema)
    {
        const string sql = @"
            SELECT 
                c.relname as tableName,
                obj_description(c.oid, 'pg_class') as description
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = @schema 
             AND c.relkind = 'r'  -- r = ordinary table
            ORDER BY c.relname;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<TableInfo>(sql, new { schema })).ToList();
    }

    public async Task<string?> GetTableDescriptionAsync(string schema, string table)
    {
        const string sql = @"
        SELECT 
            pg_catalog.obj_description(
                (quote_ident(@schema) || '.' || quote_ident(@table))::regclass::oid, 
                'pg_class'
            ) AS description;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<string>(sql, new { schema, table });
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string schema, string table)
    {
        const string sql = @"
        SELECT 
            c.column_name AS ColumnName, 
            c.data_type AS DataType, 
            (c.is_nullable = 'YES') AS IsNullable,
            (c.is_identity = 'YES') AS IsIdentity,
            c.column_default AS ColumnDefault,
            c.ordinal_position AS OrdinalPosition,
            pg_catalog.col_description(
                (quote_ident(c.table_schema) || '.' || quote_ident(c.table_name))::regclass::oid,
                c.ordinal_position
            ) AS Description
        FROM information_schema.columns c
        WHERE c.table_schema = @schema AND c.table_name = @table
        ORDER BY c.ordinal_position;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<ColumnInfo>(sql, new { schema, table })).ToList();
    }

    public async Task<List<string>> GetPrimaryKeyColumnsAsync(string schema, string table)
    {
        const string sql = @"
            SELECT kcu.column_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu 
                ON tc.constraint_name = kcu.constraint_name
                AND tc.table_schema = kcu.table_schema
            WHERE tc.table_schema = @schema 
              AND tc.table_name = @table 
              AND tc.constraint_type = 'PRIMARY KEY'
            ORDER BY kcu.ordinal_position;";

        await using var connection = new NpgsqlConnection(_connectionString);
        return (await connection.QueryAsync<string>(sql, new { schema, table })).ToList();
    }
}
