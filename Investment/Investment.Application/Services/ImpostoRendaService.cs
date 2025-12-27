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
            var trans = await _transacaoRepository.ObterPorCarteiraEAnoAsync(carteiraId, ano != null ? ano.Value : DateTime.Now.Year);
            transacoes.AddRange(trans);
        }

        if (!transacoes.Any())
            return Result<CalculoIRResponse>.Failure($"Não há transações para {(ano.HasValue ? $"o ano {ano.Value}" : "calcular")}");

        var itensCalculo = new List<ItemCalculoIR>();
        foreach (var transanao in transacoes)
        {
            var totalAtivo = transanao.Preco * transanao.Quantidade;

            itensCalculo.Add(new ItemCalculoIR
            {
                Id = Guid.NewGuid(),
                AtivoId = transanao.AtivoId,
                Quantidade = transanao.Quantidade,
                TotalInvestido = totalAtivo,
                PrecoAtual = transanao.Preco,
                PrecoMedio = 0,
                Rendimento = null,
                GanhoCapital = 0,
                IRDevido = 0,
                AliquotaIR = 0
            });
        }
        var totalTaxa = transacoes.First().Taxa;
        var totalInvestido = itensCalculo.Sum(i => i.TotalInvestido);
        foreach (var itemCalculoIR in itensCalculo)
        {
            var porcetagem = itemCalculoIR.TotalInvestido / totalInvestido;
            var taxaRateada = porcetagem * totalTaxa;
            itemCalculoIR.TaxasRateadas = taxaRateada;
            itemCalculoIR.TotalInvestido = itemCalculoIR.TotalInvestido + taxaRateada;
        }

        // Calcular totais consolidados
        var valorTotalInvestido = itensCalculo.Sum(i => i.TotalInvestido);
        var totalTaxasRateadas = itensCalculo.Sum(i => i.TaxasRateadas);
        var totalGanhoCapital = itensCalculo.Sum(i => i.GanhoCapital);
        var totalIRDevido = itensCalculo.Sum(i => i.IRDevido);

        // Verificar se já existe cálculo para este usuário e ano
        var calculoExistente = await _calculoIRRepository.ObterUltimoPorUsuarioEAnoAsync(usuarioId, ano);

        if (calculoExistente != null)
        {
            // Excluir cálculo antigo para fazer atualização
            await _calculoIRRepository.ExcluirAsync(calculoExistente.Id);
        }

        // Criar novo cálculo
        var calculoIR = new CalculoIR
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Ano = ano,
            DataCalculo = DateTimeOffset.UtcNow,
            ValorTotalInvestido = valorTotalInvestido,
            ValorTotalAtual = null,
            TotalTaxasRateadas = totalTaxasRateadas,
            TotalGanhoCapital = totalGanhoCapital,
            TotalIRDevido = totalIRDevido,
            Itens = itensCalculo
        };

        // Salvar no banco
        await _calculoIRRepository.SalvarAsync(calculoIR);

        // Recarregar do banco com propriedades de navegação (Ativo)
        var calculoSalvo = await _calculoIRRepository.ObterPorIdAsync(calculoIR.Id);
        if (calculoSalvo == null)
            return Result<CalculoIRResponse>.Failure("Erro ao salvar cálculo de IR");

        // Mapear para DTO
        var itens = calculoSalvo.Itens.Select(i => new ItemCalculoIRResponse
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
        }).ToList();
        var response = new CalculoIRResponse
        {
            Id = calculoSalvo.Id,
            Ano = calculoSalvo.Ano,
            DataCalculo = calculoSalvo.DataCalculo,
            ValorTotalInvestido = calculoSalvo.ValorTotalInvestido,
            ValorTotalAtual = calculoSalvo.ValorTotalAtual,
            TotalTaxasRateadas = calculoSalvo.TotalTaxasRateadas,
            TotalGanhoCapital = calculoSalvo.TotalGanhoCapital,
            TotalIRDevido = calculoSalvo.TotalIRDevido,
            Itens = itens
        };

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

            // Calcular rateio para o histórico (mesmo algoritmo conforme @regras.md)
            var comprasPorMes = comprasAtivo
                .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                .ToList();

            var historicoCompras = new List<HistoricoCompraResponse>();

            foreach (var mesGrupo in comprasPorMes)
            {
                var comprasMes = mesGrupo.OrderBy(t => t.DataTransacao).ToList();

                // Total investido sem taxas
                var totalMes = comprasMes.Sum(c => c.Preco * c.Quantidade);

                // Taxa da PRIMEIRA transação do mês (conforme @regras.md)
                var totalTaxasMes = comprasMes.First().Taxa;

                foreach (var compra in comprasMes)
                {
                    // Total do ativo = Quantidade × Preço
                    var totalAtivo = compra.Preco * compra.Quantidade;

                    // % do Ativo = Total do Ativo / Total investido (sem taxas)
                    var percentualAtivo = totalMes > 0 ? totalAtivo / totalMes : 0;

                    // Taxa de Rateio = % do Ativo × Total de Taxas
                    var taxaRateada = percentualAtivo * totalTaxasMes;

                    // Total com Taxas = Taxa Rateio + Total Ativo
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
