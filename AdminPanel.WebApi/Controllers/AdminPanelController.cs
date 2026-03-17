using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Models;
using AdminPanel.Domain.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminPanel.WebApi.Controllers;

[Route("api/admin")]
public class AdminPanelController : ControllerBase
{
    private readonly IDatabaseSchemaService _schemaService;
    private readonly ITableDataService _dataService;

    public AdminPanelController(IDatabaseSchemaService schemaService, ITableDataService dataService)
    {
        _schemaService = schemaService;
        _dataService = dataService;
    }

    [HttpGet("schemas")]
    public async Task<IActionResult> GetSchemas()
    {
        var schemas = await _schemaService.GetSchemasAsync();
        return Ok(schemas);
    }

    [HttpGet("schemaDescription/{schema}")]
    public async Task<IActionResult> GetSchemaDescription(string schema)
    {
        var description = await _schemaService.GetSchemaDescriptionAsync(schema);
        return Ok(description);
    }

    [HttpGet("schemas/{schema}/tables")]
    public async Task<IActionResult> GetTables(string schema)
    {
        var tables = await _schemaService.GetTablesAsync(schema);
        return Ok(tables);
    }

    [HttpGet("tableDescription/{schema}/{table}")]
    public async Task<IActionResult> GetTableDescription(string schema, string table)
    {
        var description = await _schemaService.GetTableDescriptionAsync(schema, table);
        return Ok(description);
    }

    [HttpGet("schemas/{schema}/tables/{table}/columns")]
    public async Task<IActionResult> GetColumns(string schema, string table)
    {
        var columns = await _schemaService.GetColumnsAsync(schema, table);
        return Ok(columns);
    }

    [HttpGet("schemas/{schema}/tables/{table}/primarykeys")]
    public async Task<IActionResult> GetPrimaryKeys(string schema, string table)
    {
        var keys = await _schemaService.GetPrimaryKeyColumnsAsync(schema, table);
        return Ok(keys);
    }

    [HttpPost("schemas/{schema}/tables/{table}/data")]
    public async Task<IActionResult> GetData(
        string schema,
        string table,
        [FromBody] DataRequest request)
    {
        var columns = await _schemaService.GetColumnsAsync(schema, table);
        var data = await _dataService.GetDataAsync(
            schema,
            table,
            request.OrderByColumn ?? "id",
            request.Filters,
            columns,
            request.Page,
            request.PageSize);
        return Ok(data);
    }

    [HttpPost("schemas/{schema}/tables/{table}/count")]
    public async Task<IActionResult> GetCount(
        string schema,
        string table,
        [FromBody] List<SearchCondition> filters)
    {
        var columns = await _schemaService.GetColumnsAsync(schema, table);
        var count = await _dataService.GetCountAsync(schema, table, filters, columns);
        return Ok(count);
    }

    [HttpPost("schemas/{schema}/tables/{table}/insert")]
    public async Task<IActionResult> Insert(
        string schema,
        string table,
        [FromBody] Dictionary<string, object> values)
    {
        try
        {
            var columns = await _schemaService.GetColumnsAsync(schema, table);

            var convertedValues = new Dictionary<string, object>();
            foreach (var kv in values)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == kv.Key);
                if (col == null)
                    return BadRequest($"Column '{kv.Key}' not found in table {schema}.{table}");

                object convertedValue = kv.Value is JsonElement e
                    ? ConvertJsonElement(e, col.DataType)
                    : kv.Value;
                convertedValues[kv.Key] = convertedValue;
            }

            await _dataService.InsertAsync(schema, table, convertedValues);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("schemas/{schema}/tables/{table}/update")]
    public async Task<IActionResult> Update(
        string schema,
        string table,
        [FromBody] UpdateRequest request)
    {
        try
        {
            var columns = await _schemaService.GetColumnsAsync(schema, table);

            var convertedValues = new Dictionary<string, object>();
            foreach (var kv in request.Values)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == kv.Key);
                if (col == null)
                    return BadRequest($"Column '{kv.Key}' not found in table {schema}.{table}");

                object convertedValue = kv.Value is JsonElement e
                    ? ConvertJsonElement(e, col.DataType)
                    : kv.Value;
                convertedValues[kv.Key] = convertedValue;
            }

            var convertedKeyValues = new Dictionary<string, object>();
            foreach (var kv in request.KeyValues)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == kv.Key);
                if (col == null)
                    return BadRequest($"Column '{kv.Key}' not found in table {schema}.{table}");

                object convertedValue = kv.Value is JsonElement e
                    ? ConvertJsonElement(e, col.DataType)
                    : kv.Value;
                convertedKeyValues[kv.Key] = convertedValue;
            }

            var updateRequest = new UpdateRequest
            {
                Values = convertedValues,
                KeyColumns = request.KeyColumns,
                KeyValues = convertedKeyValues
            };

            await _dataService.UpdateAsync(
                schema,
                table,
                updateRequest.Values,
                updateRequest.KeyColumns,
                updateRequest.KeyValues);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

    }

    [HttpDelete("schemas/{schema}/tables/{table}/delete")]
    public async Task<IActionResult> Delete(
        string schema,
        string table,
        [FromBody] Dictionary<string, object> keyValues)
    {

        var columns = await _schemaService.GetColumnsAsync(schema, table);

        var convertedKeyValues = new Dictionary<string, object>();
        foreach (var kv in keyValues)
        {
            var col = columns.FirstOrDefault(c => c.ColumnName == kv.Key);
            if (col == null)
                return BadRequest($"Колонки '{kv.Key}' нет в таблице {schema}.{table}");

            object convertedValue;
            if (kv.Value is JsonElement element)
            {
                convertedValue = ConvertJsonElement(element, col.DataType);
            }
            else
            {
                convertedValue = kv.Value;
            }
            convertedKeyValues[kv.Key] = convertedValue;
        }

        await _dataService.DeleteAsync(schema, table, convertedKeyValues);
        return Ok();
    }

    private object ConvertJsonElement(JsonElement element, string dataType)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var lowerType = dataType.ToLowerInvariant();
                if (lowerType.Contains("date") || lowerType.Contains("timestamp"))
                    return element.GetDateTime();
                if (lowerType.Contains("guid") || lowerType.Contains("uuid"))
                    return element.GetGuid();
                return element.GetString();

            case JsonValueKind.Number:
                if (dataType.Contains("int") || dataType.Contains("integer"))
                    return element.GetInt32();
                if (dataType.Contains("bigint"))
                    return element.GetInt64();
                if (dataType.Contains("decimal") || dataType.Contains("numeric"))
                    return element.GetDecimal();
                if (dataType.Contains("float") || dataType.Contains("double"))
                    return element.GetDouble();
                return element.GetRawText();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();

            case JsonValueKind.Null:
                return null;

            default:
                return element.GetRawText();
        }
    }
}
