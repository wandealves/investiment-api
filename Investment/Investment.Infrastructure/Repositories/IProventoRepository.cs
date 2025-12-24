using Gridify;
using Investment.Domain.Common;
using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface IProventoRepository
{
    Task<Provento?> ObterPorIdAsync(long id);
    Task<List<Provento>> ObterTodosAsync();
    Task<Paging<Provento>> ObterAsync(GridifyQuery query);
    Task<List<Provento>> ObterPorAtivoIdAsync(long ativoId);
    Task<List<Provento>> ObterPorStatusAsync(StatusProvento status);
    Task<List<Provento>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim);
    Task<List<Provento>> ObterPorAtivoEPeriodoAsync(long ativoId, DateTimeOffset inicio, DateTimeOffset fim);
    Task<List<Provento>> ObterAgendadosAsync();
    Task<List<Provento>> ObterPagosPorAtivoAsync(long ativoId);
    Task<Provento?> ObterComDetalhesAsync(long id);
    Task<Provento> SalvarAsync(Provento provento);
    Task<Provento> AtualizarAsync(Provento provento);
    Task<bool> ExcluirAsync(long id);
}
