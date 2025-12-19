using Gridify;
using Investment.Application.DTOs.Transacao;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface ITransacaoService
{
    Task<Result<TransacaoResponse>> ObterPorIdAsync(Guid id, Guid usuarioId);
    Task<Result<Paging<TransacaoResponse>>> ObterPorCarteiraAsync(long carteiraId, GridifyQuery query, Guid usuarioId);
    Task<Result<List<TransacaoResponse>>> ObterPorPeriodoAsync(long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId);
    Task<Result<TransacaoResponse>> CriarAsync(TransacaoRequest request, Guid usuarioId);
    Task<Result<TransacaoResponse>> AtualizarAsync(Guid id, TransacaoRequest request, Guid usuarioId);
    Task<Result> ExcluirAsync(Guid id, Guid usuarioId);
}
