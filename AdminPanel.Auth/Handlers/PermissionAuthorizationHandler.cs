using AdminPanel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AdminPanel.Auth.Handlers;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionClaim = context.User.FindFirst("permissions");
        if (permissionClaim != null && int.TryParse(permissionClaim.Value, out int permissions))
        {
            if (((Permission)permissions).HasFlag(requirement.RequiredPermission))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}

