using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CommonDbContext _dbContext;
    private readonly DbSet<User> _users;

    public UserRepository(CommonDbContext dbContext)
    {
        _dbContext = dbContext;
        _users = dbContext.Set<User>();
    }

    public void Add(User user)
    {
        _users.Add(user);
    }

    public async Task<User?> FirstOrDefaultAsync(string login)
    {
        return await _users.FirstOrDefaultAsync(x => x.Login == login);
    }

    public async Task<User?> GetById(int id)
    {
        return await _users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<User>> ListAsync()
    {
        return await _users.ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
