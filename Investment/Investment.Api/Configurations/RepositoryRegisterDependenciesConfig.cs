using Investment.Infrastructure.Repositories;

namespace Investment.Api.Configurations;
public static class RepositoryRegisterDependenciesConfig
{
    public static void RegisterRepository(this IServiceCollection services)
    {
        services.AddScoped<IAtivoRepository, AtivoRepository>();
        services.AddScoped<ICarteiraRepository, CarteiraRepository>();
        services.AddScoped<ICarteiraAtivoRepository, CarteiraAtivoRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICotacaoRepository, CotacaoRepository>();
        services.AddScoped<IProventoRepository, ProventoRepository>();
    }
}

