namespace AdminPanel.Models;

public class DataRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string OrderByColumn { get; set; }
    public List<SearchCondition> Filters { get; set; } = new();
}
