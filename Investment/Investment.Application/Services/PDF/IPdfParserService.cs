using Investment.Domain.Common;
using Investment.Domain.ValueObjects;

namespace Investment.Application.Services.PDF;

public interface IPdfParserService
{
    Task<Result<NotaCorretagem>> ParseNotaAsync(Stream pdfStream, string corretoraTipo);
}
