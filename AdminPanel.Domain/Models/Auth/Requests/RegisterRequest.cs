using AdminPanel.Domain.Enums;

namespace AdminPanel.Domain.Models.Auth.Requests;

public class RegisterRequest
{
    public string FullName { get; set; }
    public string Login { get; set; }
    public Permission Permissions { get; set; }
}