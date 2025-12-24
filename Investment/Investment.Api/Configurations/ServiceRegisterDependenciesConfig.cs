using Investment.Application.Services;
using Investment.Application.Services.Cotacao;
using Investment.Application.Services.PDF;

namespace Investment.Api.Configurations;
public static class ServiceRegisterDependenciesConfig
{
    public static void RegisterService(this IServiceCollection services)
    {
        services.AddScoped<IAtivoService, AtivoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ICarteiraService, CarteiraService>();
        services.AddScoped<ITransacaoService, TransacaoService>();
        services.AddScoped<IPosicaoService, PosicaoService>();
        services.AddScoped<ILookupService, LookupService>();


        // Registrar serviços de cotação
        services.AddScoped<ICotacaoService, CotacaoService>();
        services.AddHttpClient<ICotacaoProviderStrategy, BrapiProvider>(client =>
        {
            client.BaseAddress = new Uri("https://brapi.dev/api/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Registrar serviços de importação PDF
        services.AddScoped<IPdfParserStrategy, ClearPdfParser>();
        services.AddScoped<IPdfParserStrategy, XPPdfParser>();
        services.AddScoped<IPdfParserService, PdfParserService>();
        services.AddScoped<IImportacaoService, ImportacaoService>();

        // Registrar serviços de relatórios
        services.AddScoped<IRelatorioService, RelatorioService>();

        // Registrar serviços de dashboard
        services.AddScoped<IDashboardService, DashboardService>();
    }
}

