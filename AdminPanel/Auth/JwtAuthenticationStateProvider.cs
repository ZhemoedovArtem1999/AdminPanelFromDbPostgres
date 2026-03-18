using AdminPanel.Domain.Models.Auth.Requests;
using AdminPanel.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace AdminPanel.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IAuthApi _api;
    private static readonly AuthenticationState _unauthenticated = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(IJSRuntime jsRuntime, IAuthApi api)
    {
        _jsRuntime = jsRuntime;
        _api = api;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessToken();
        if (string.IsNullOrEmpty(token))
            return _unauthenticated;

        if (IsTokenExpired(token))
        {
            token = await TryRefreshToken();
            if (string.IsNullOrEmpty(token))
                return _unauthenticated;
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private async Task<string> GetAccessToken()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
    }

    private async Task<string> GetRefreshToken()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
    }

    private bool IsTokenExpired(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (exp == null) return true;
        var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
        return expiry < DateTimeOffset.UtcNow;
    }

    private async Task<string> TryRefreshToken()
    {
        var refreshToken = await GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        try
        {
            var response = await _api.RefreshAsync(new RefreshTokenRequest { RefreshToken = refreshToken });
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", response.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", response.RefreshToken);
            return response.AccessToken;
        }
        catch
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            return null;
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(_unauthenticated));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var payload = token.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
