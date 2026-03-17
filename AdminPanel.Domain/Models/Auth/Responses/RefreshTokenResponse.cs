namespace AdminPanel.Domain.Models.Auth.Responses;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
