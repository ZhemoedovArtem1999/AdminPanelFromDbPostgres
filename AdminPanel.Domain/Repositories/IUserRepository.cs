using AdminPanel.Domain.Entities;

namespace AdminPanel.Domain.Repositories;

public interface IUserRepository
{
    Task<User> FirstOrDefaultAsync(string login);
    Task<IEnumerable<User>> ListAsync();
    Task<User> GetById(int id);
    void Add(User user);

    Task SaveAsync();
}
