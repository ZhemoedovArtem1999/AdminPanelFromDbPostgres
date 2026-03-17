using AdminPanel.Domain.Entities;

namespace AdminPanel.Domain.Interfaces.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
