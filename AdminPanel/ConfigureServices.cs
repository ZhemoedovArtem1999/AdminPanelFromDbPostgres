using AdminPanel.Auth;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Interfaces;
using AdminPanel.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace AdminPanel;

public static class ConfigureServices
{
    public static void ConfigureAdminPanelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDatabaseSchemaService, DatabaseSchemaService>();
        services.AddScoped<ITableDataService, TableDataService>();

        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddAuthorizationCore();

        services.AddTransient<AuthenticatedHttpClientHandler>();

        services.AddRefitClient<IAdminPanelApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl не указан в конфигурации")))
            .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

        services.AddRefitClient<IAuthApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl не указан в конфигурации")));

        services.AddRefitClient<IUsersApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl не указан в конфигурации")))
            .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

    }
}
