using Investment.Application.DTOs.Posicao;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IPosicaoService
{
    Task<Result<PosicaoConsolidadaResponse>> CalcularPosicaoAsync(long carteiraId, Guid usuarioId);
    Task<Result<PosicaoAtivoResponse>> CalcularPosicaoAtivoAsync(long carteiraId, long ativoId, Guid usuarioId);
    Task<Result<List<PosicaoConsolidadaResponse>>> CalcularTodasPosicoesAsync(Guid usuarioId);
}
