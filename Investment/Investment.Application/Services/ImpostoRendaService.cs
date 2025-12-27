using Investment.Application.DTOs.ImpostoRenda;
using Investment.Application.Services.Cotacao;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class ImpostoRendaService : IImpostoRendaService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly ICalculoIRRepository _calculoIRRepository;
    private readonly ICotacaoService _cotacaoService;

    public ImpostoRendaService(
        ITransacaoRepository transacaoRepository,
        ICarteiraRepository carteiraRepository,
        ICalculoIRRepository calculoIRRepository,
        ICotacaoService cotacaoService)
    {
        _transacaoRepository = transacaoRepository;
        _carteiraRepository = carteiraRepository;
        _calculoIRRepository = calculoIRRepository;
        _cotacaoService = cotacaoService;
    }

    public async Task<Result<CalculoIRResponse>> CalcularIRAsync(int? ano, Guid usuarioId)
    {
        // 1. Obter todas as carteiras do usuário
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        if (!carteiras.Any())
            return Result<CalculoIRResponse>.Failure("Usuário não possui carteiras");

        var carteiraIds = carteiras.Select(c => c.Id).ToList();

        // 2. Obter todas as transações (filtradas por ano se especificado)
        var transacoes = new List<Transacao>();
        foreach (var carteiraId in carteiraIds)
        {
            var trans = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);
            transacoes.AddRange(trans);
        }

        if (ano.HasValue)
        {
            transacoes = transacoes
                .Where(t => t.DataTransacao.Year == ano.Value)
                .ToList();
        }

        if (!transacoes.Any())
            return Result<CalculoIRResponse>.Failure($"Não há transações para {(ano.HasValue ? $"o ano {ano.Value}" : "calcular")}");

        // 3. Agrupar por ativo
        var transacoesPorAtivo = transacoes
            .GroupBy(t => t.AtivoId)
            .ToList();

        var itensCalculo = new List<ItemCalculoIR>();

        // 4. Para cada ativo, calcular
        foreach (var grupo in transacoesPorAtivo)
        {
            var ativoId = grupo.Key;
            var transacoesAtivo = grupo.OrderBy(t => t.DataTransacao).ToList();
            var ativo = transacoesAtivo.First().Ativo;

            // 4.1. Separar compras por mês para rateio de taxas
            var comprasPorMes = transacoesAtivo
                .Where(t => t.TipoTransacao == TipoTransacao.Compra)
                .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                .ToList();

            // 4.2. Aplicar rateio de taxas conforme @regras.md
            var comprasComRateio = new List<(Transacao Transacao, decimal TaxaRateada)>();

            foreach (var mesGrupo in comprasPorMes)
            {
                var comprasMes = mesGrupo.ToList();

                // Total do mês = Σ (Preço × Quantidade)
                var totalMes = comprasMes.Sum(c => c.Preco * c.Quantidade);

                // Total de taxas do mês = Σ Taxas
                var totalTaxasMes = comprasMes.Sum(c => c.Taxa);

                foreach (var compra in comprasMes)
                {
                    // Total do ativo
                    var totalAtivo = compra.Preco * compra.Quantidade;

                    // % do Ativo = Total do Ativo / Total do Mês
                    var percentualAtivo = totalMes > 0 ? totalAtivo / totalMes : 0;

                    // Taxa de Rateio = % do Ativo × Total de Taxas
                    var taxaRateada = percentualAtivo * totalTaxasMes;

                    comprasComRateio.Add((compra, taxaRateada));
                }
            }

            // 4.3. Calcular WAC (Weighted Average Cost) com taxas rateadas
            decimal quantidadeAtual = 0;
            decimal precoMedio = 0;
            decimal totalTaxasRateadas = 0;

            foreach (var (transacao, taxaRateada) in comprasComRateio.OrderBy(c => c.Transacao.DataTransacao))
            {
                var precoComTaxa = transacao.Preco + (taxaRateada / transacao.Quantidade);

                if (quantidadeAtual + transacao.Quantidade > 0)
                {
                    precoMedio = ((quantidadeAtual * precoMedio) + (transacao.Quantidade * precoComTaxa))
                               / (quantidadeAtual + transacao.Quantidade);
                }
                else
                {
                    precoMedio = precoComTaxa;
                }

                quantidadeAtual += transacao.Quantidade;
                totalTaxasRateadas += taxaRateada;
            }

            // 4.4. Processar vendas para calcular ganho de capital
            var vendas = transacoesAtivo.Where(t => t.TipoTransacao == TipoTransacao.Venda).ToList();
            decimal ganhoCapital = 0;

            foreach (var venda in vendas)
            {
                var quantidadeVendida = Math.Abs(venda.Quantidade);
                var precoVenda = venda.Preco;

                // Ganho de Capital = (Preço Venda - Preço Médio) × Quantidade
                ganhoCapital += (precoVenda - precoMedio) * quantidadeVendida;

                // Atualizar quantidade
                quantidadeAtual -= quantidadeVendida;
            }

            // 4.5. Buscar cotação atual via BRAPI
            decimal? precoAtual = null;
            try
            {
                precoAtual = await _cotacaoService.ObterPrecoAtualAsync(ativoId);
            }
            catch
            {
                // Ignora erro de cotação - continua sem preço atual
            }

            // 4.6. Calcular IR devido
            var (irDevido, aliquota) = CalcularIRDevido(ativo.Tipo, ganhoCapital, vendas);

            // 4.7. Criar item de cálculo
            var totalInvestido = quantidadeAtual * precoMedio;
            var valorAtual = precoAtual.HasValue ? quantidadeAtual * precoAtual.Value : (decimal?)null;
            var rendimento = valorAtual.HasValue ? valorAtual.Value - totalInvestido : (decimal?)null;

            var itemCalculo = new ItemCalculoIR
            {
                Id = Guid.NewGuid(),
                AtivoId = ativoId,
                Quantidade = quantidadeAtual,
                TotalInvestido = totalInvestido,
                PrecoMedio = precoMedio,
                PrecoAtual = precoAtual,
                Rendimento = rendimento,
                TaxasRateadas = totalTaxasRateadas,
                GanhoCapital = ganhoCapital,
                IRDevido = irDevido,
                AliquotaIR = aliquota
                // Ativo será carregado pelo EF Core via Include, não definir aqui
            };

            itensCalculo.Add(itemCalculo);
        }

        // 5. Criar CalculoIR
        var calculoIR = new CalculoIR
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Ano = ano,
            DataCalculo = DateTimeOffset.UtcNow,
            ValorTotalInvestido = itensCalculo.Sum(i => i.TotalInvestido),
            ValorTotalAtual = itensCalculo.Any(i => i.PrecoAtual.HasValue)
                ? itensCalculo.Sum(i => i.PrecoAtual.HasValue ? i.Quantidade * i.PrecoAtual.Value : 0)
                : null,
            TotalTaxasRateadas = itensCalculo.Sum(i => i.TaxasRateadas),
            TotalGanhoCapital = itensCalculo.Sum(i => i.GanhoCapital),
            TotalIRDevido = itensCalculo.Sum(i => i.IRDevido),
            Itens = itensCalculo
        };

        // 6. Salvar no banco
        await _calculoIRRepository.SalvarAsync(calculoIR);

        // 7. Recarregar do banco com propriedades de navegação
        var calculoSalvo = await _calculoIRRepository.ObterPorIdAsync(calculoIR.Id);
        if (calculoSalvo == null)
            return Result<CalculoIRResponse>.Failure("Erro ao salvar cálculo de IR");

        // 8. Mapear para DTO com histórico de compras
        var response = MapearParaResponse(calculoSalvo, transacoes);

        return Result<CalculoIRResponse>.Success(response);
    }

    private (decimal IRDevido, decimal Aliquota) CalcularIRDevido(TipoAtivo tipo, decimal ganhoCapital, List<Transacao> vendas)
    {
        if (ganhoCapital <= 0)
            return (0, 0);

        decimal aliquota = 0;

        switch (tipo)
        {
            case TipoAtivo.Acao:
                // Verificar isenção de R$20.000/mês
                var vendasPorMes = vendas
                    .GroupBy(v => new { v.DataTransacao.Year, v.DataTransacao.Month })
                    .ToList();

                decimal ganhoTributavel = 0;
                foreach (var mesGrupo in vendasPorMes)
                {
                    var totalVendasMes = mesGrupo.Sum(v => Math.Abs(v.Quantidade) * v.Preco);
                    if (totalVendasMes > 20000)
                    {
                        // Proporção tributável
                        var ganhoMes = ganhoCapital / vendas.Count * mesGrupo.Count();
                        ganhoTributavel += ganhoMes;
                    }
                }

                aliquota = ganhoTributavel > 0 ? 15m : 0m;
                return (ganhoTributavel * aliquota / 100, aliquota);

            case TipoAtivo.FII:
                aliquota = 20m;
                return (ganhoCapital * aliquota / 100, aliquota);

            case TipoAtivo.ETF:
                aliquota = 15m;
                return (ganhoCapital * aliquota / 100, aliquota);

            default:
                return (0, 0);
        }
    }

    private CalculoIRResponse MapearParaResponse(CalculoIR calculoIR, List<Transacao> todasTransacoes)
    {
        var itensResponse = new List<ItemCalculoIRResponse>();

        foreach (var item in calculoIR.Itens)
        {
            // Buscar histórico de compras do ativo
            var comprasAtivo = todasTransacoes
                .Where(t => t.AtivoId == item.AtivoId && t.TipoTransacao == TipoTransacao.Compra)
                .OrderBy(t => t.DataTransacao)
                .ToList();

            // Calcular rateio para o histórico (mesmo algoritmo)
            var comprasPorMes = comprasAtivo
                .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                .ToList();

            var historicoCompras = new List<HistoricoCompraResponse>();

            foreach (var mesGrupo in comprasPorMes)
            {
                var comprasMes = mesGrupo.ToList();
                var totalMes = comprasMes.Sum(c => c.Preco * c.Quantidade);
                var totalTaxasMes = comprasMes.Sum(c => c.Taxa);

                foreach (var compra in comprasMes)
                {
                    var totalAtivo = compra.Preco * compra.Quantidade;
                    var percentualAtivo = totalMes > 0 ? totalAtivo / totalMes : 0;
                    var taxaRateada = percentualAtivo * totalTaxasMes;
                    var totalComTaxas = totalAtivo + taxaRateada;

                    historicoCompras.Add(new HistoricoCompraResponse
                    {
                        TransacaoId = compra.Id,
                        DataCompra = compra.DataTransacao,
                        Quantidade = compra.Quantidade,
                        Preco = compra.Preco,
                        Taxa = compra.Taxa,
                        TaxaRateada = taxaRateada,
                        TotalComTaxas = totalComTaxas
                    });
                }
            }

            itensResponse.Add(new ItemCalculoIRResponse
            {
                AtivoId = item.AtivoId,
                AtivoNome = item.Ativo.Nome,
                AtivoCodigo = item.Ativo.Codigo,
                AtivoTipo = item.Ativo.Tipo,
                Quantidade = item.Quantidade,
                TotalInvestido = item.TotalInvestido,
                PrecoMedio = item.PrecoMedio,
                PrecoAtual = item.PrecoAtual,
                Rendimento = item.Rendimento,
                TaxasRateadas = item.TaxasRateadas,
                GanhoCapital = item.GanhoCapital,
                IRDevido = item.IRDevido,
                AliquotaIR = item.AliquotaIR,
                HistoricoCompras = historicoCompras
            });
        }

        return new CalculoIRResponse
        {
            Id = calculoIR.Id,
            Ano = calculoIR.Ano,
            DataCalculo = calculoIR.DataCalculo,
            ValorTotalInvestido = calculoIR.ValorTotalInvestido,
            ValorTotalAtual = calculoIR.ValorTotalAtual,
            TotalTaxasRateadas = calculoIR.TotalTaxasRateadas,
            TotalGanhoCapital = calculoIR.TotalGanhoCapital,
            TotalIRDevido = calculoIR.TotalIRDevido,
            Itens = itensResponse
        };
    }

    public async Task<Result<CalculoIRResponse>> RecalcularIRAsync(Guid calculoId, Guid usuarioId)
    {
        var calculoExistente = await _calculoIRRepository.ObterPorIdAsync(calculoId);
        if (calculoExistente == null)
            return Result<CalculoIRResponse>.Failure("Cálculo não encontrado");

        if (calculoExistente.UsuarioId != usuarioId)
            return Result<CalculoIRResponse>.Failure("Acesso negado");

        // Excluir cálculo antigo e criar novo
        await _calculoIRRepository.ExcluirAsync(calculoId);
        return await CalcularIRAsync(calculoExistente.Ano, usuarioId);
    }

    public async Task<Result<List<CalculoIRResponse>>> ObterHistoricoCalculosAsync(Guid usuarioId)
    {
        var calculos = await _calculoIRRepository.ObterPorUsuarioIdAsync(usuarioId);

        var response = calculos.Select(c => new CalculoIRResponse
        {
            Id = c.Id,
            Ano = c.Ano,
            DataCalculo = c.DataCalculo,
            ValorTotalInvestido = c.ValorTotalInvestido,
            ValorTotalAtual = c.ValorTotalAtual,
            TotalTaxasRateadas = c.TotalTaxasRateadas,
            TotalGanhoCapital = c.TotalGanhoCapital,
            TotalIRDevido = c.TotalIRDevido,
            Itens = c.Itens.Select(i => new ItemCalculoIRResponse
            {
                AtivoId = i.AtivoId,
                AtivoNome = i.Ativo.Nome,
                AtivoCodigo = i.Ativo.Codigo,
                AtivoTipo = i.Ativo.Tipo,
                Quantidade = i.Quantidade,
                TotalInvestido = i.TotalInvestido,
                PrecoMedio = i.PrecoMedio,
                PrecoAtual = i.PrecoAtual,
                Rendimento = i.Rendimento,
                TaxasRateadas = i.TaxasRateadas,
                GanhoCapital = i.GanhoCapital,
                IRDevido = i.IRDevido,
                AliquotaIR = i.AliquotaIR,
                HistoricoCompras = new List<HistoricoCompraResponse>()
            }).ToList()
        }).ToList();

        return Result<List<CalculoIRResponse>>.Success(response);
    }

    public async Task<Result> ExcluirCalculoAsync(Guid calculoId, Guid usuarioId)
    {
        var calculo = await _calculoIRRepository.ObterPorIdAsync(calculoId);
        if (calculo == null)
            return Result.Failure("Cálculo não encontrado");

        if (calculo.UsuarioId != usuarioId)
            return Result.Failure("Acesso negado");

        await _calculoIRRepository.ExcluirAsync(calculoId);
        return Result.Success();
    }
}
