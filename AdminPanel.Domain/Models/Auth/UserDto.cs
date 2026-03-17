using AdminPanel.Domain.Enums;

namespace AdminPanel.Domain.Models.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public Permission Permissions { get; set; }
    public bool PasswordSet { get; set; }
}
