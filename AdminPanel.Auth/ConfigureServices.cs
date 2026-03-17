using AdminPanel.Auth.Handlers;
using AdminPanel.Auth.Services;
using AdminPanel.Domain.Enums;
using AdminPanel.Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AdminPanel.Auth;

public static class ConfigureServices
{
    public static void ConfigureAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("View", policy => policy.Requirements.Add(new PermissionRequirement(Permission.View)));
            options.AddPolicy("Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permission.Edit)));
            options.AddPolicy("Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permission.Delete)));
            options.AddPolicy("ManageUsers", policy => policy.Requirements.Add(new PermissionRequirement(Permission.ManageUsers)));
        });
    }
}
