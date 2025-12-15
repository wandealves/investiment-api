using Investment.Application.DTOs.Dashboard;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IPosicaoService _posicaoService;
    private readonly ITransacaoRepository _transacaoRepository;

    public DashboardService(
        ICarteiraRepository carteiraRepository,
        IPosicaoService posicaoService,
        ITransacaoRepository transacaoRepository)
    {
        _carteiraRepository = carteiraRepository;
        _posicaoService = posicaoService;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<Result<DashboardMetricsResponse>> ObterMetricasAsync(Guid usuarioId)
    {
        // Obter todas as carteiras do usuário
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        var quantidadeCarteiras = carteiras.Count;

        if (quantidadeCarteiras == 0)
        {
            return Result<DashboardMetricsResponse>.Success(new DashboardMetricsResponse
            {
                PatrimonioTotal = 0,
                RentabilidadeTotal = 0,
                QuantidadeCarteiras = 0,
                QuantidadeAtivos = 0,
                MelhorAtivo = null,
                PiorAtivo = null
            });
        }

        // Calcular posições consolidadas de todas as carteiras
        var todasPosicoes = new List<(string Codigo, decimal Rentabilidade, decimal ValorAtual)>();
        decimal patrimonioTotal = 0;
        decimal valorInvestidoTotal = 0;

        foreach (var carteira in carteiras)
        {
            var resultadoPosicao = await _posicaoService.CalcularPosicaoAsync(carteira.Id, usuarioId);

            if (!resultadoPosicao.IsSuccess || resultadoPosicao.Data == null)
                continue;

            var posicaoConsolidada = resultadoPosicao.Data;

            foreach (var posicao in posicaoConsolidada.Posicoes)
            {
                patrimonioTotal += posicao.ValorAtual ?? 0;
                valorInvestidoTotal += posicao.ValorInvestido;

                if (posicao.Rentabilidade.HasValue)
                {
                    todasPosicoes.Add((posicao.AtivoCodigo, posicao.Rentabilidade.Value, posicao.ValorAtual ?? 0));
                }
            }
        }

        // Calcular rentabilidade total
        var rentabilidadeTotal = valorInvestidoTotal > 0
            ? ((patrimonioTotal - valorInvestidoTotal) / valorInvestidoTotal) * 100
            : 0;

        // Identificar melhor e pior ativo
        MelhorPiorAtivoResponse? melhorAtivo = null;
        MelhorPiorAtivoResponse? piorAtivo = null;

        if (todasPosicoes.Any())
        {
            var melhor = todasPosicoes.OrderByDescending(p => p.Rentabilidade).First();
            var pior = todasPosicoes.OrderBy(p => p.Rentabilidade).First();

            melhorAtivo = new MelhorPiorAtivoResponse
            {
                Codigo = melhor.Codigo,
                Rentabilidade = melhor.Rentabilidade
            };

            piorAtivo = new MelhorPiorAtivoResponse
            {
                Codigo = pior.Codigo,
                Rentabilidade = pior.Rentabilidade
            };
        }

        // Contar ativos únicos
        var quantidadeAtivos = todasPosicoes.Select(p => p.Codigo).Distinct().Count();

        var metricas = new DashboardMetricsResponse
        {
            PatrimonioTotal = patrimonioTotal,
            RentabilidadeTotal = rentabilidadeTotal,
            QuantidadeCarteiras = quantidadeCarteiras,
            QuantidadeAtivos = quantidadeAtivos,
            MelhorAtivo = melhorAtivo,
            PiorAtivo = piorAtivo
        };

        return Result<DashboardMetricsResponse>.Success(metricas);
    }

    public async Task<Result<List<AlocacaoResponse>>> ObterAlocacaoAsync(Guid usuarioId)
    {
        // Obter todas as carteiras do usuário
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);

        if (!carteiras.Any())
        {
            return Result<List<AlocacaoResponse>>.Success(new List<AlocacaoResponse>());
        }

        // Agrupar por tipo de ativo
        var alocacaoPorTipo = new Dictionary<string, decimal>();
        decimal totalGeral = 0;

        foreach (var carteira in carteiras)
        {
            var resultadoPosicao = await _posicaoService.CalcularPosicaoAsync(carteira.Id, usuarioId);

            if (!resultadoPosicao.IsSuccess || resultadoPosicao.Data == null)
                continue;

            foreach (var posicao in resultadoPosicao.Data.Posicoes)
            {
                var valorAtual = posicao.ValorAtual ?? 0;
                var tipo = posicao.AtivoTipo;

                if (!alocacaoPorTipo.ContainsKey(tipo))
                {
                    alocacaoPorTipo[tipo] = 0;
                }

                alocacaoPorTipo[tipo] += valorAtual;
                totalGeral += valorAtual;
            }
        }

        // Calcular percentuais
        var alocacao = alocacaoPorTipo
            .Select(kvp => new AlocacaoResponse
            {
                Tipo = kvp.Key,
                Valor = kvp.Value,
                Percentual = totalGeral > 0 ? (kvp.Value / totalGeral) * 100 : 0
            })
            .OrderByDescending(a => a.Valor)
            .ToList();

        return Result<List<AlocacaoResponse>>.Success(alocacao);
    }

    public async Task<Result<List<EvolucaoPatrimonioResponse>>> ObterEvolucaoPatrimonialAsync(Guid usuarioId)
    {
        // Obter todas as carteiras do usuário
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);

        if (!carteiras.Any())
        {
            return Result<List<EvolucaoPatrimonioResponse>>.Success(new List<EvolucaoPatrimonioResponse>());
        }

        // Obter todas as transações do usuário
        var todasTransacoes = new List<Investment.Domain.Entidades.Transacao>();

        foreach (var carteira in carteiras)
        {
            var transacoes = await _transacaoRepository.ObterPorCarteiraIdAsync(carteira.Id);
            todasTransacoes.AddRange(transacoes);
        }

        if (!todasTransacoes.Any())
        {
            return Result<List<EvolucaoPatrimonioResponse>>.Success(new List<EvolucaoPatrimonioResponse>());
        }

        // Agrupar por mês e calcular patrimônio acumulado
        var transacoesOrdenadas = todasTransacoes
            .OrderBy(t => t.DataTransacao)
            .ToList();

        var dataInicio = transacoesOrdenadas.First().DataTransacao;
        var dataFim = DateTimeOffset.Now;

        var evolucao = new List<EvolucaoPatrimonioResponse>();
        var patrimonioAcumulado = new Dictionary<long, (decimal Quantidade, decimal PrecoMedio)>();

        // Iterar mês a mês
        var dataAtual = new DateTimeOffset(dataInicio.Year, dataInicio.Month, 1, 0, 0, 0, dataInicio.Offset);

        while (dataAtual <= dataFim)
        {
            var proximoMes = dataAtual.AddMonths(1);

            // Processar transações do mês
            var transacoesMes = transacoesOrdenadas
                .Where(t => t.DataTransacao >= dataAtual && t.DataTransacao < proximoMes)
                .ToList();

            foreach (var transacao in transacoesMes)
            {
                if (!patrimonioAcumulado.ContainsKey(transacao.AtivoId))
                {
                    patrimonioAcumulado[transacao.AtivoId] = (0, 0);
                }

                var (quantidadeAtual, precoMedioAtual) = patrimonioAcumulado[transacao.AtivoId];

                if (transacao.TipoTransacao == "Compra")
                {
                    var novaQuantidade = quantidadeAtual + transacao.Quantidade;
                    var novoPrecoMedio = novaQuantidade > 0
                        ? ((quantidadeAtual * precoMedioAtual) + (transacao.Quantidade * transacao.Preco)) / novaQuantidade
                        : 0;

                    patrimonioAcumulado[transacao.AtivoId] = (novaQuantidade, novoPrecoMedio);
                }
                else if (transacao.TipoTransacao == "Venda")
                {
                    var novaQuantidade = quantidadeAtual - transacao.Quantidade;
                    patrimonioAcumulado[transacao.AtivoId] = (novaQuantidade, precoMedioAtual);
                }
            }

            // Calcular valor total do patrimônio no final do mês
            var valorPatrimonio = patrimonioAcumulado
                .Where(kvp => kvp.Value.Quantidade > 0)
                .Sum(kvp => kvp.Value.Quantidade * kvp.Value.PrecoMedio);

            evolucao.Add(new EvolucaoPatrimonioResponse
            {
                Data = dataAtual.ToString("yyyy-MM"),
                Valor = valorPatrimonio
            });

            dataAtual = proximoMes;
        }

        return Result<List<EvolucaoPatrimonioResponse>>.Success(evolucao);
    }
}
