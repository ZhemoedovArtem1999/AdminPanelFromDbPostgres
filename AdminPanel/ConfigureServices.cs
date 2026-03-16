using AdminPanel.Interfaces;
using AdminPanel.Services;
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

        services.AddRefitClient<IAdminPanelApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl не указан в конфигурации")));

    }
}
