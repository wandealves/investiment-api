using Investment.Application.DTOs.Relatorio;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IRelatorioService
{
    Task<Result<RelatorioRentabilidadeResponse>> GerarRelatorioRentabilidadeAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId);

    Task<Result<RelatorioProventosResponse>> GerarRelatorioProventosAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId);

    Task<Result<byte[]>> ExportarRelatorioPdfAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId);

    Task<Result<byte[]>> ExportarRelatorioExcelAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId);
}
