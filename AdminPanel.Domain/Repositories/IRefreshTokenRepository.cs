using AdminPanel.Domain.Entities;

namespace AdminPanel.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> FirstOrDefaultIncludeUserAsync(string refreshToken); // Include User
                                                                            // .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
    Task<RefreshToken> FirstOrDefaultAsync(string refreshToken);
    // FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
    void Add(RefreshToken refreshToken);

    Task SaveAsync();

}
