namespace AdminPanel.Domain.Enums;

[Flags]
public enum Permission
{
    None = 0,
    View = 1 << 0,
    Edit = 1 << 1,
    Delete = 1 << 2,
    ManageUsers = 1 << 3
}
