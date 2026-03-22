using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CommonDbContext _dbContext;
    private readonly DbSet<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(CommonDbContext dbContext)
    {
        _dbContext = dbContext;
        _refreshTokens = dbContext.Set<RefreshToken>();
    }

    public void Add(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
    }

    public async Task<RefreshToken?> FirstOrDefaultAsync(string refreshToken)
    {
        return await _refreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
    }

    public async Task<RefreshToken?> FirstOrDefaultIncludeUserAsync(string refreshToken)
    {
        return await _refreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
