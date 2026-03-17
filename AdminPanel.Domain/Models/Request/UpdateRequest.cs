namespace AdminPanel.Domain.Models.Request;

public class UpdateRequest
{
    public Dictionary<string, object> Values { get; set; }
    public List<string> KeyColumns { get; set; }
    public Dictionary<string, object> KeyValues { get; set; }
}
