namespace AdminPanel.WebApi.Models
{
    public class SearchCondition
    {
        public string FieldName { get; set; }
        public string Operator { get; set; } // "contains", "equals", "startswith", "gt", "lt"
        public string Value { get; set; }
    }
}
