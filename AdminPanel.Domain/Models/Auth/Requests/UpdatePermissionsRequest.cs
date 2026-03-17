using AdminPanel.Domain.Enums;

namespace AdminPanel.Domain.Models.Auth.Requests;

public class UpdatePermissionsRequest
{
    public Permission Permissions { get; set; }
}
