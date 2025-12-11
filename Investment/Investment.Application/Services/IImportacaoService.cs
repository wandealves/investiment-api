using Investment.Application.DTOs.Importacao;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IImportacaoService
{
    Task<Result<ImportacaoResponse>> PreviewImportacaoAsync(
        long carteiraId, Stream pdfStream, string corretoraTipo, Guid usuarioId);

    Task<Result<ImportacaoResponse>> ImportarNotaAsync(
        long carteiraId, Stream pdfStream, string corretoraTipo, Guid usuarioId);
}
