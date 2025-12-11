using Investment.Application.DTOs.Relatorio;
using Investment.Application.Services.Calculos;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Investment.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly IPosicaoService _posicaoService;
    private readonly ICarteiraRepository _carteiraRepository;

    public RelatorioService(
        ITransacaoRepository transacaoRepository,
        IPosicaoService posicaoService,
        ICarteiraRepository carteiraRepository)
    {
        _transacaoRepository = transacaoRepository;
        _posicaoService = posicaoService;
        _carteiraRepository = carteiraRepository;
    }

    public async Task<Result<RelatorioRentabilidadeResponse>> GerarRelatorioRentabilidadeAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<RelatorioRentabilidadeResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteira = await _carteiraRepository.ObterPorIdAsync(carteiraId);
        if (carteira == null)
        {
            return Result<RelatorioRentabilidadeResponse>.Failure($"Carteira com ID {carteiraId} não encontrada");
        }

        // Buscar transações do período
        var dataInicio = new DateTimeOffset(inicio, TimeSpan.Zero);
        var dataFim = new DateTimeOffset(fim.AddDays(1).AddTicks(-1), TimeSpan.Zero); // Fim do dia

        var transacoesPeriodo = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);
        var transacoesNoPeriodo = transacoesPeriodo
            .Where(t => t.DataTransacao >= dataInicio && t.DataTransacao <= dataFim)
            .OrderBy(t => t.DataTransacao)
            .ToList();

        // Calcular cash flows para IRR
        var cashFlows = new List<(DateTime Data, decimal Valor)>();
        decimal totalAportes = 0m;
        decimal totalResgates = 0m;
        decimal totalProventos = 0m;

        foreach (var transacao in transacoesNoPeriodo)
        {
            var data = transacao.DataTransacao.DateTime;

            switch (transacao.TipoTransacao)
            {
                case TipoTransacao.Compra:
                    var valorCompra = transacao.Quantidade * transacao.Preco;
                    cashFlows.Add((data, -valorCompra)); // Negativo = saída de caixa
                    totalAportes += valorCompra;
                    break;

                case TipoTransacao.Venda:
                    var valorVenda = Math.Abs(transacao.Quantidade) * transacao.Preco;
                    cashFlows.Add((data, valorVenda)); // Positivo = entrada de caixa
                    totalResgates += valorVenda;
                    break;

                case TipoTransacao.Dividendo:
                case TipoTransacao.JCP:
                    var valorProvento = transacao.Preco * Math.Abs(transacao.Quantidade);
                    cashFlows.Add((data, valorProvento));
                    totalProventos += valorProvento;
                    break;
            }
        }

        // Calcular posição atual (valor final)
        var posicaoAtual = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);
        var valorFinal = posicaoAtual.Data?.ValorTotalInvestido ?? 0m;

        // Valor inicial (simplificado: valor final - aportes + resgates - proventos)
        var valorInicial = valorFinal - totalAportes + totalResgates - totalProventos;
        if (valorInicial < 0) valorInicial = 0m;

        // Adicionar valor final aos cash flows para IRR
        if (valorFinal > 0)
        {
            cashFlows.Add((fim, valorFinal));
        }

        // Calcular IRR
        decimal? irr = null;
        if (cashFlows.Count >= 2)
        {
            irr = IrrCalculator.Calculate(cashFlows);
        }

        // Calcular TWR
        decimal? twr = null;
        if (valorInicial > 0)
        {
            twr = TwrCalculator.CalculateSimple(valorInicial, valorFinal, totalAportes, totalResgates);
        }

        // Retorno simples
        var retornoSimples = valorInicial > 0
            ? ((valorFinal - valorInicial + totalProventos) / valorInicial) * 100m
            : 0m;

        // Agrupar por mês para rendimento mensal
        var rendimentoPorMes = CalcularRendimentoMensal(transacoesNoPeriodo, inicio, fim);

        var response = new RelatorioRentabilidadeResponse
        {
            CarteiraId = carteiraId,
            CarteiraNome = carteira.Nome,
            DataInicio = dataInicio,
            DataFim = dataFim,
            IRR = irr,
            TWR = twr,
            RetornoSimples = retornoSimples,
            ValorInicial = valorInicial,
            ValorFinal = valorFinal,
            Aportes = totalAportes,
            Resgates = totalResgates,
            Proventos = totalProventos,
            RendimentoPorMes = rendimentoPorMes
        };

        return Result<RelatorioRentabilidadeResponse>.Success(response);
    }

    public async Task<Result<RelatorioProventosResponse>> GerarRelatorioProventosAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<RelatorioProventosResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteira = await _carteiraRepository.ObterPorIdAsync(carteiraId);
        if (carteira == null)
        {
            return Result<RelatorioProventosResponse>.Failure($"Carteira com ID {carteiraId} não encontrada");
        }

        // Buscar transações do período
        var dataInicio = new DateTimeOffset(inicio, TimeSpan.Zero);
        var dataFim = new DateTimeOffset(fim.AddDays(1).AddTicks(-1), TimeSpan.Zero);

        var transacoes = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);
        var proventos = transacoes
            .Where(t => t.DataTransacao >= dataInicio && t.DataTransacao <= dataFim &&
                       (t.TipoTransacao == TipoTransacao.Dividendo || t.TipoTransacao == TipoTransacao.JCP))
            .ToList();

        // Agrupar por ativo
        var proventosPorAtivo = proventos
            .GroupBy(t => t.AtivoId)
            .Select(g =>
            {
                var primeiraTransacao = g.First();
                var totalRecebido = g.Sum(t => t.Preco * Math.Abs(t.Quantidade));
                var dataUltimo = g.Max(t => t.DataTransacao);

                return new ProventoAtivoResponse
                {
                    AtivoId = g.Key,
                    Nome = primeiraTransacao.Ativo?.Nome ?? "",
                    Codigo = primeiraTransacao.Ativo?.Codigo ?? "",
                    TotalRecebido = totalRecebido,
                    Quantidade = g.Count(),
                    DataUltimoPagamento = dataUltimo
                };
            })
            .OrderByDescending(p => p.TotalRecebido)
            .ToList();

        var totalDividendos = proventos
            .Where(t => t.TipoTransacao == TipoTransacao.Dividendo)
            .Sum(t => t.Preco * Math.Abs(t.Quantidade));

        var totalJCP = proventos
            .Where(t => t.TipoTransacao == TipoTransacao.JCP)
            .Sum(t => t.Preco * Math.Abs(t.Quantidade));

        var response = new RelatorioProventosResponse
        {
            CarteiraId = carteiraId,
            CarteiraNome = carteira.Nome,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ProventosPorAtivo = proventosPorAtivo,
            TotalDividendos = totalDividendos,
            TotalJCP = totalJCP,
            TotalGeral = totalDividendos + totalJCP
        };

        return Result<RelatorioProventosResponse>.Success(response);
    }

    public async Task<Result<byte[]>> ExportarRelatorioPdfAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId)
    {
        var relatorioRentabilidade = await GerarRelatorioRentabilidadeAsync(carteiraId, inicio, fim, usuarioId);
        if (!relatorioRentabilidade.IsSuccess || relatorioRentabilidade.Data == null)
        {
            return Result<byte[]>.Failure(relatorioRentabilidade.Errors);
        }

        var relatorioProventos = await GerarRelatorioProventosAsync(carteiraId, inicio, fim, usuarioId);

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Relatório de Investimentos - {relatorioRentabilidade.Data.CarteiraNome}")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Item().Text($"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}").FontSize(12);
                            col.Item().PaddingTop(10);

                            // Seção de Rentabilidade
                            col.Item().Text("Rentabilidade").SemiBold().FontSize(14);
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Valor Inicial:");
                                table.Cell().Element(CellStyle).Text($"R$ {relatorioRentabilidade.Data.ValorInicial:N2}");

                                table.Cell().Element(CellStyle).Text("Valor Final:");
                                table.Cell().Element(CellStyle).Text($"R$ {relatorioRentabilidade.Data.ValorFinal:N2}");

                                table.Cell().Element(CellStyle).Text("Aportes:");
                                table.Cell().Element(CellStyle).Text($"R$ {relatorioRentabilidade.Data.Aportes:N2}");

                                table.Cell().Element(CellStyle).Text("Resgates:");
                                table.Cell().Element(CellStyle).Text($"R$ {relatorioRentabilidade.Data.Resgates:N2}");

                                table.Cell().Element(CellStyle).Text("Proventos:");
                                table.Cell().Element(CellStyle).Text($"R$ {relatorioRentabilidade.Data.Proventos:N2}");

                                table.Cell().Element(CellStyle).Text("Retorno Simples:");
                                table.Cell().Element(CellStyle).Text($"{relatorioRentabilidade.Data.RetornoSimples:N2}%");

                                if (relatorioRentabilidade.Data.IRR.HasValue)
                                {
                                    table.Cell().Element(CellStyle).Text("IRR (Anualizado):");
                                    table.Cell().Element(CellStyle).Text($"{relatorioRentabilidade.Data.IRR.Value:N2}%");
                                }

                                if (relatorioRentabilidade.Data.TWR.HasValue)
                                {
                                    table.Cell().Element(CellStyle).Text("TWR:");
                                    table.Cell().Element(CellStyle).Text($"{relatorioRentabilidade.Data.TWR.Value:N2}%");
                                }
                            });

                            // Seção de Proventos
                            if (relatorioProventos.IsSuccess && relatorioProventos.Data != null)
                            {
                                col.Item().PaddingTop(20).Text("Proventos Recebidos").SemiBold().FontSize(14);
                                col.Item().PaddingTop(5).Text($"Total: R$ {relatorioProventos.Data.TotalGeral:N2}");
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Gerado em: ");
                            x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                });
            }).GeneratePdf();

            return Result<byte[]>.Success(pdf);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Erro ao gerar PDF: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> ExportarRelatorioExcelAsync(
        long carteiraId, DateTime inicio, DateTime fim, Guid usuarioId)
    {
        var relatorioRentabilidade = await GerarRelatorioRentabilidadeAsync(carteiraId, inicio, fim, usuarioId);
        if (!relatorioRentabilidade.IsSuccess || relatorioRentabilidade.Data == null)
        {
            return Result<byte[]>.Failure(relatorioRentabilidade.Errors);
        }

        var relatorioProventos = await GerarRelatorioProventosAsync(carteiraId, inicio, fim, usuarioId);
        var posicaoAtual = await _posicaoService.CalcularPosicaoAsync(carteiraId, usuarioId);

        try
        {
            using var workbook = new XLWorkbook();

            // Sheet 1: Resumo
            var sheetResumo = workbook.Worksheets.Add("Resumo");
            sheetResumo.Cell(1, 1).Value = "Relatório de Investimentos";
            sheetResumo.Cell(1, 1).Style.Font.Bold = true;
            sheetResumo.Cell(1, 1).Style.Font.FontSize = 14;

            sheetResumo.Cell(2, 1).Value = $"Carteira: {relatorioRentabilidade.Data.CarteiraNome}";
            sheetResumo.Cell(3, 1).Value = $"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}";

            int row = 5;
            sheetResumo.Cell(row++, 1).Value = "Métrica";
            sheetResumo.Cell(row - 1, 2).Value = "Valor";
            sheetResumo.Range(row - 1, 1, row - 1, 2).Style.Font.Bold = true;

            sheetResumo.Cell(row, 1).Value = "Valor Inicial";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.ValorInicial;

            sheetResumo.Cell(row, 1).Value = "Valor Final";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.ValorFinal;

            sheetResumo.Cell(row, 1).Value = "Aportes";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.Aportes;

            sheetResumo.Cell(row, 1).Value = "Resgates";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.Resgates;

            sheetResumo.Cell(row, 1).Value = "Proventos";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.Proventos;

            sheetResumo.Cell(row, 1).Value = "Retorno Simples (%)";
            sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.RetornoSimples;

            if (relatorioRentabilidade.Data.IRR.HasValue)
            {
                sheetResumo.Cell(row, 1).Value = "IRR - Anualizado (%)";
                sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.IRR.Value;
            }

            if (relatorioRentabilidade.Data.TWR.HasValue)
            {
                sheetResumo.Cell(row, 1).Value = "TWR (%)";
                sheetResumo.Cell(row++, 2).Value = relatorioRentabilidade.Data.TWR.Value;
            }

            sheetResumo.Columns().AdjustToContents();

            // Sheet 2: Proventos
            if (relatorioProventos.IsSuccess && relatorioProventos.Data != null)
            {
                var sheetProventos = workbook.Worksheets.Add("Proventos");
                sheetProventos.Cell(1, 1).Value = "Ativo";
                sheetProventos.Cell(1, 2).Value = "Código";
                sheetProventos.Cell(1, 3).Value = "Total Recebido";
                sheetProventos.Cell(1, 4).Value = "Quantidade";
                sheetProventos.Range(1, 1, 1, 4).Style.Font.Bold = true;

                row = 2;
                foreach (var provento in relatorioProventos.Data.ProventosPorAtivo)
                {
                    sheetProventos.Cell(row, 1).Value = provento.Nome;
                    sheetProventos.Cell(row, 2).Value = provento.Codigo;
                    sheetProventos.Cell(row, 3).Value = provento.TotalRecebido;
                    sheetProventos.Cell(row, 4).Value = provento.Quantidade;
                    row++;
                }

                sheetProventos.Columns().AdjustToContents();
            }

            // Sheet 3: Posição Atual
            if (posicaoAtual.IsSuccess && posicaoAtual.Data != null)
            {
                var sheetPosicao = workbook.Worksheets.Add("Posição Atual");
                sheetPosicao.Cell(1, 1).Value = "Ativo";
                sheetPosicao.Cell(1, 2).Value = "Código";
                sheetPosicao.Cell(1, 3).Value = "Quantidade";
                sheetPosicao.Cell(1, 4).Value = "Preço Médio";
                sheetPosicao.Cell(1, 5).Value = "Valor Investido";
                sheetPosicao.Range(1, 1, 1, 5).Style.Font.Bold = true;

                row = 2;
                foreach (var posicao in posicaoAtual.Data.Posicoes)
                {
                    sheetPosicao.Cell(row, 1).Value = posicao.AtivoNome;
                    sheetPosicao.Cell(row, 2).Value = posicao.AtivoCodigo;
                    sheetPosicao.Cell(row, 3).Value = posicao.QuantidadeAtual;
                    sheetPosicao.Cell(row, 4).Value = posicao.PrecoMedio;
                    sheetPosicao.Cell(row, 5).Value = posicao.ValorInvestido;
                    row++;
                }

                sheetPosicao.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Result<byte[]>.Success(stream.ToArray());
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Erro ao gerar Excel: {ex.Message}");
        }
    }

    private List<RendimentoMensalResponse> CalcularRendimentoMensal(
        List<Domain.Entidades.Transacao> transacoes, DateTime inicio, DateTime fim)
    {
        var rendimentos = new List<RendimentoMensalResponse>();

        var dataAtual = new DateTime(inicio.Year, inicio.Month, 1);
        var dataFinal = new DateTime(fim.Year, fim.Month, 1);

        while (dataAtual <= dataFinal)
        {
            var mesKey = dataAtual.ToString("yyyy-MM");
            var inicioDomes = dataAtual;
            var fimDoMes = dataAtual.AddMonths(1).AddDays(-1);

            var transacoesMes = transacoes
                .Where(t => t.DataTransacao.DateTime >= inicioDomes && t.DataTransacao.DateTime <= fimDoMes)
                .ToList();

            // Cálculo simplificado de rentabilidade mensal
            var valorInvestidoMes = transacoesMes
                .Where(t => t.TipoTransacao == TipoTransacao.Compra)
                .Sum(t => t.Quantidade * t.Preco);

            rendimentos.Add(new RendimentoMensalResponse
            {
                Mes = mesKey,
                Rentabilidade = 0m, // Simplificado - requer dados históricos de valoração
                ValorFinal = valorInvestidoMes
            });

            dataAtual = dataAtual.AddMonths(1);
        }

        return rendimentos;
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}
