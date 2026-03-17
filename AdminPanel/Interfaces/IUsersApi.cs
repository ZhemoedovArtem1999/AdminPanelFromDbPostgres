using AdminPanel.Domain.Models.Auth;
using AdminPanel.Domain.Models.Auth.Requests;
using Refit;

namespace AdminPanel.Interfaces;

public interface IUsersApi
{
    [Get("/api/users")]
    Task<List<UserDto>> GetUsersAsync();

    [Put("/api/users/{id}/permissions")]
    Task UpdateUserPermissionsAsync(int id, [Body] UpdatePermissionsRequest request);
}
