
using AdminPanel.WebApi.Components;
using AdminPanel.WebApi.Services;
using Npgsql;

namespace AdminPanel.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
        AppContext.SetSwitch("Npgsql.EnableSqlLogging", true);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        #region подключение Blazor

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddHttpClient();

        builder.Services.AddScoped<DatabaseSchemaService>();
        builder.Services.AddScoped<TableDataService>();
        #endregion

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        // для Blazor
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAntiforgery();

        // авторизация / аутентификация если нужна будет

        app.MapControllers();

        // для Blazor
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
