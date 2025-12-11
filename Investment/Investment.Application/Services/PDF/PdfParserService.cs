using Investment.Domain.Common;
using Investment.Domain.ValueObjects;

namespace Investment.Application.Services.PDF;

public class PdfParserService : IPdfParserService
{
    private readonly Dictionary<string, IPdfParserStrategy> _parsers;

    public PdfParserService(IEnumerable<IPdfParserStrategy> parsers)
    {
        _parsers = parsers.ToDictionary(p => p.CorretoraNome, p => p, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Result<NotaCorretagem>> ParseNotaAsync(Stream pdfStream, string corretoraTipo)
    {
        if (!_parsers.TryGetValue(corretoraTipo, out var parser))
        {
            return Result<NotaCorretagem>.Failure($"Parser para corretora '{corretoraTipo}' não encontrado. Corretoras suportadas: {string.Join(", ", _parsers.Keys)}");
        }

        try
        {
            var nota = await parser.ParseAsync(pdfStream);

            if (string.IsNullOrEmpty(nota.NumeroNota) || nota.NumeroNota == "N/A")
            {
                return Result<NotaCorretagem>.Failure("Não foi possível extrair o número da nota. Verifique se o PDF está no formato correto.");
            }

            if (nota.Operacoes.Count == 0)
            {
                return Result<NotaCorretagem>.Failure("Nenhuma operação encontrada no PDF. Verifique se o PDF contém transações válidas.");
            }

            return Result<NotaCorretagem>.Success(nota);
        }
        catch (Exception ex)
        {
            return Result<NotaCorretagem>.Failure($"Erro ao processar PDF: {ex.Message}");
        }
    }
}
