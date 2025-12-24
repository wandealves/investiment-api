using Investment.Api.Endpoints;

namespace Investment.Api.Configurations;
public static class EndpointRegisterDependenciesConfig
{
    public static void UseEndpointConfiguration(this WebApplication app)
    {
        app.RegistrarAuthEndpoints();
        app.RegistrarUsuarioEndpoints();
        app.RegistrarCarteiraEndpoints();
        app.RegistrarTransacaoEndpoints();
        app.RegistrarPosicaoEndpoints();
        app.RegistrarAtivoEndpoints();
        app.RegistrarImportacaoEndpoints();
        app.RegistrarRelatorioEndpoints();
        app.RegistrarDashboardEndpoints();
        app.RegistrarCotacaoEndpoints();
        app.RegistrarLookupEndpoints();
    }
}

