using AdminPanel.Domain.Enums;

namespace AdminPanel.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public Permission Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool PasswordSet => !string.IsNullOrEmpty(PasswordHash);

    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
