using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface ICotacaoRepository
{
    Task<Cotacao> SalvarAsync(Cotacao cotacao);
    Task<Cotacao?> ObterPorIdAsync(long id);
    Task<List<Cotacao>> ObterPorAtivoIdAsync(long ativoId);
    Task<List<Cotacao>> ObterPorAtivoEPeriodoAsync(long ativoId, DateTimeOffset inicio, DateTimeOffset fim);
    Task<Cotacao?> ObterUltimaCotacaoAsync(long ativoId);
    Task<bool> ExcluirAsync(long id);
}
