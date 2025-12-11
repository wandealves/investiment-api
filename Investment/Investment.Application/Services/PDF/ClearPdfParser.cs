using System.Globalization;
using System.Text.RegularExpressions;
using Investment.Domain.ValueObjects;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Investment.Application.Services.PDF;

public class ClearPdfParser : IPdfParserStrategy
{
    public string CorretoraNome => "Clear";

    public async Task<NotaCorretagem> ParseAsync(Stream pdfStream)
    {
        return await Task.Run(() =>
        {
            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var texto = ExtrairTextoCompleto(pdfDocument);

            var nota = new NotaCorretagem
            {
                Corretora = "Clear",
                NumeroNota = ExtrairNumeroNota(texto),
                DataPregao = ExtrairDataPregao(texto),
                Operacoes = ExtrairOperacoes(texto),
                Custos = ExtrairCustos(texto)
            };

            DistribuirCustosProporcionalmente(nota);

            return nota;
        });
    }

    private string ExtrairTextoCompleto(PdfDocument pdfDocument)
    {
        var texto = string.Empty;
        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var strategy = new SimpleTextExtractionStrategy();
            texto += PdfTextExtractor.GetTextFromPage(page, strategy);
        }
        return texto;
    }

    private string ExtrairNumeroNota(string texto)
    {
        var match = Regex.Match(texto, @"Nr\.\s*nota\s*(\d+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "N/A";
    }

    private DateTimeOffset ExtrairDataPregao(string texto)
    {
        var match = Regex.Match(texto, @"Data\s+preg[aã]o\s*[:.]?\s*(\d{2}[/\-]\d{2}[/\-]\d{4})", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var dataStr = match.Groups[1].Value;
            if (DateTime.TryParseExact(dataStr, new[] { "dd/MM/yyyy", "dd-MM-yyyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                return new DateTimeOffset(data, TimeSpan.Zero);
            }
        }

        return DateTimeOffset.UtcNow;
    }

    private List<OperacaoNota> ExtrairOperacoes(string texto)
    {
        var operacoes = new List<OperacaoNota>();

        // Padrão para operações Clear: (C|V) TICKER QUANTIDADE PRECO VALOR
        // Exemplo: C PETR4 100 28,50 2.850,00
        var pattern = @"(C|V)\s+([A-Z]{4}\d{1,2}[A-Z]?)\s+([\d.]+)\s+([\d.,]+)\s+([\d.,]+)";
        var matches = Regex.Matches(texto, pattern);

        foreach (Match match in matches)
        {
            try
            {
                var tipoOperacao = match.Groups[1].Value;
                var ticker = match.Groups[2].Value;
                var quantidade = ParseDecimal(match.Groups[3].Value);
                var preco = ParseDecimal(match.Groups[4].Value);
                var valorTotal = ParseDecimal(match.Groups[5].Value);

                operacoes.Add(new OperacaoNota
                {
                    TipoOperacao = tipoOperacao,
                    Ticker = ticker,
                    Quantidade = quantidade,
                    PrecoUnitario = preco,
                    ValorTotal = valorTotal,
                    TaxasProporcionais = 0 // Será calculado depois
                });
            }
            catch
            {
                // Ignora operações com formato inválido
                continue;
            }
        }

        return operacoes;
    }

    private CustosNota ExtrairCustos(string texto)
    {
        var custos = new CustosNota();

        // Buscar custos no rodapé da nota
        custos.TaxaLiquidacao = ExtrairValor(texto, @"Taxa\s+de\s+liquida[cç][aã]o\s+([\d.,]+)");
        custos.Emolumentos = ExtrairValor(texto, @"Emolumentos\s+([\d.,]+)");
        custos.ISS = ExtrairValor(texto, @"I\.?S\.?S\.?\s+([\d.,]+)");
        custos.Corretagem = ExtrairValor(texto, @"Corretagem\s+([\d.,]+)");
        custos.OutrosCustos = ExtrairValor(texto, @"Outros\s+([\d.,]+)");

        custos.Total = custos.TaxaLiquidacao + custos.Emolumentos +
                       custos.ISS + custos.Corretagem + custos.OutrosCustos;

        return custos;
    }

    private decimal ExtrairValor(string texto, string pattern)
    {
        var match = Regex.Match(texto, pattern, RegexOptions.IgnoreCase);
        return match.Success ? ParseDecimal(match.Groups[1].Value) : 0m;
    }

    private decimal ParseDecimal(string valor)
    {
        // Remove pontos de milhar e substitui vírgula por ponto
        valor = valor.Replace(".", "").Replace(",", ".");
        return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado)
            ? resultado : 0m;
    }

    private void DistribuirCustosProporcionalmente(NotaCorretagem nota)
    {
        if (nota.Operacoes.Count == 0 || nota.Custos.Total == 0)
            return;

        var valorTotalOperacoes = nota.Operacoes.Sum(o => o.ValorTotal);

        if (valorTotalOperacoes == 0)
            return;

        foreach (var operacao in nota.Operacoes)
        {
            var proporcao = operacao.ValorTotal / valorTotalOperacoes;
            operacao.TaxasProporcionais = nota.Custos.Total * proporcao;
        }
    }
}
