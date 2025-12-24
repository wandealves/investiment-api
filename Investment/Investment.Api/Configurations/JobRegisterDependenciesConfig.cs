using Hangfire;
using Hangfire.PostgreSql;
using Investment.Api.Infrastructure;
using Investment.Application.Services.Cotacao;

namespace Investment.Api.Configurations;
public static class JobRegisterDependenciesConfig
{
    public static void RegisterJob(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Configurar Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(configuration.GetConnectionString("Default")!)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2; // 2 workers paralelos
            options.ServerName = "InvestmentApi-CotacaoWorker";
        });
    }

    public static void UseJobConfiguration(this WebApplication app)
    {
        // Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            DashboardTitle = "Investment API - Cotações"
        });

        // Agendar job recorrente de atualização de cotações
        RecurringJob.AddOrUpdate<ICotacaoService>(
            "atualizar-cotacoes",
            service => service.AtualizarTodasCotacoesAsync(),
            Cron.Daily(14, 10), // Todos os dias às 18:30
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time") // Brasília
            });

    }
}

