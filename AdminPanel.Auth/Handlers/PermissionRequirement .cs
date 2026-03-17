using AdminPanel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AdminPanel.Auth.Handlers;

public class PermissionRequirement : IAuthorizationRequirement
{
    public Permission RequiredPermission { get; }
    public PermissionRequirement(Permission permission) => RequiredPermission = permission;
}

