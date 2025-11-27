using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface ICarteiraAtivoRepository
{
    Task<CarteiraAtivo?> ObterPorIdAsync(long id);
    Task<List<CarteiraAtivo>> ObterPorCarteiraIdAsync(long carteiraId);
    Task<List<CarteiraAtivo>> ObterPorAtivoIdAsync(long ativoId);
    Task<CarteiraAtivo?> ObterPorCarteiraEAtivoAsync(long carteiraId, long ativoId);
    Task<List<CarteiraAtivo>> ObterComDetalhesAsync(long carteiraId);
    Task<CarteiraAtivo> SalvarAsync(CarteiraAtivo carteiraAtivo);
    Task<bool> ExcluirAsync(long id);
    Task<bool> RemoverPorCarteiraEAtivoAsync(long carteiraId, long ativoId);
    Task<bool> ExistePorCarteiraEAtivoAsync(long carteiraId, long ativoId);
}
