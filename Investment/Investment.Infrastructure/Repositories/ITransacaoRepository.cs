using Gridify;
using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface ITransacaoRepository
{
    Task<Transacao?> ObterPorIdAsync(Guid id);
    Task<List<Transacao>> ObterTodosAsync();
    Task<Paging<Transacao>> ObterAsync(GridifyQuery query);
    Task<List<Transacao>> ObterPorCarteiraIdAsync(long carteiraId);
    Task<Paging<Transacao>> ObterPorCarteiraAsync(long carteiraId, GridifyQuery query);
    Task<Paging<Transacao>> ObterPorUsuarioAsync(Guid usuarioId, GridifyQuery query);
    Task<List<Transacao>> ObterPorAtivoIdAsync(long ativoId);
    Task<List<Transacao>> ObterPorCarteiraEAtivoAsync(long carteiraId, long ativoId);
    Task<List<Transacao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<List<Transacao>> ObterPorCarteiraEPeriodoAsync(long carteiraId, DateTime inicio, DateTime fim);
    Task<List<Transacao>> ObterPorTipoAsync(string tipoTransacao);
    Task<List<Transacao>> ObterPorCarteiraTipoEPeriodoAsync(long carteiraId, string tipoTransacao, DateTime inicio, DateTime fim);
    Task<Transacao?> ObterComDetalhesAsync(Guid id);
    Task<List<Transacao>> ObterUltimasTransacoesAsync(long carteiraId, int quantidade);
    Task<decimal> CalcularTotalInvestidoPorAtivoAsync(long carteiraId, long ativoId);
    Task<Transacao> SalvarAsync(Transacao transacao);
    Task<Transacao> AtualizarAsync(Transacao transacao);
    Task<bool> ExcluirAsync(Guid id);
}
