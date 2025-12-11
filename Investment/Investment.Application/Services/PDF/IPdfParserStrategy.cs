using Investment.Domain.ValueObjects;

namespace Investment.Application.Services.PDF;

public interface IPdfParserStrategy
{
    Task<NotaCorretagem> ParseAsync(Stream pdfStream);
    string CorretoraNome { get; }
}
