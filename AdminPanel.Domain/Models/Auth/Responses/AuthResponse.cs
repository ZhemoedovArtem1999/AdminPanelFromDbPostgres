using AdminPanel.Domain.Enums;

namespace AdminPanel.Domain.Models.Auth.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string FullName { get; set; }
    public Permission Permissions { get; set; }
    public bool RequiresPasswordSetup { get; set; }
}
