using AdminPanel.Domain.Models.Auth;
using AdminPanel.Domain.Models.Auth.Requests;
using AdminPanel.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "ManageUsers")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.ListAsync();

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Login = u.Login,
            Permissions = u.Permissions,
            PasswordSet = u.PasswordSet
        });

        return Ok(userDtos);
    }

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> UpdatePermissions(int id, [FromBody] UpdatePermissionsRequest request)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        user.Permissions = request.Permissions;
        await _userRepository.SaveAsync();
        return Ok();
    }
}