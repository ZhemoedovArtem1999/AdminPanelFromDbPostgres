namespace AdminPanel.Domain.Models.Auth.Requests;

public class SetPasswordRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
