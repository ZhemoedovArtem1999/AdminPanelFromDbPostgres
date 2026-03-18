using AdminPanel.Auth;
using Npgsql;
using Persistence;

namespace AdminPanel.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
        //AppContext.SetSwitch("Npgsql.EnableSqlLogging", true);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddHttpClient();
        builder.Services.ConfigureAdminPanelServices(builder.Configuration);
        builder.Services.ConfigurePersistenceServices(builder.Configuration);
        builder.Services.ConfigureAuthServices(builder.Configuration);

        builder.Services.AddControllers();


        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAntiforgery();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapRazorComponents<AdminPanel.Components.App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
