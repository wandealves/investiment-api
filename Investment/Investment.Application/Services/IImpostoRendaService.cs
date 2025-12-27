using Investment.Application.DTOs.ImpostoRenda;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IImpostoRendaService
{
    Task<Result<CalculoIRResponse>> CalcularIRAsync(int? ano, Guid usuarioId);
    Task<Result<CalculoIRResponse>> RecalcularIRAsync(Guid calculoId, Guid usuarioId);
    Task<Result<List<CalculoIRResponse>>> ObterHistoricoCalculosAsync(Guid usuarioId);
    Task<Result> ExcluirCalculoAsync(Guid calculoId, Guid usuarioId);
}
