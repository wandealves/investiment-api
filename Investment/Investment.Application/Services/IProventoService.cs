using Gridify;
using Investment.Application.DTOs.Provento;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IProventoService
{
    Task<Result<ProventoResponse>> ObterPorIdAsync(long id);
    Task<Result<Paging<ProventoResponse>>> ObterAsync(GridifyQuery query);
    Task<Result<List<ProventoResponse>>> ObterPorAtivoAsync(long ativoId);
    Task<Result<List<ProventoResponse>>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim);
    Task<Result<List<ProventoResponse>>> ObterAgendadosAsync();
    Task<Result<ProventoComTransacoesResponse>> ObterComTransacoesAsync(long id);
    Task<Result<ProventoResponse>> CriarAsync(ProventoRequest request);
    Task<Result<ProventoResponse>> AtualizarAsync(long id, ProventoRequest request);
    Task<Result> ExcluirAsync(long id);
    Task<Result<ProventoResponse>> MarcarComoPagoAsync(long id);
    Task<Result<ProventoResponse>> CancelarAsync(long id);
}
