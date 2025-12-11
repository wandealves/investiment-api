using Investment.Application.DTOs.Posicao;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class PosicaoService : IPosicaoService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IAtivoRepository _ativoRepository;

    public PosicaoService(
        ITransacaoRepository transacaoRepository,
        ICarteiraRepository carteiraRepository,
        IAtivoRepository ativoRepository)
    {
        _transacaoRepository = transacaoRepository;
        _carteiraRepository = carteiraRepository;
        _ativoRepository = ativoRepository;
    }

    public async Task<Result<PosicaoConsolidadaResponse>> CalcularPosicaoAsync(long carteiraId, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<PosicaoConsolidadaResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        var carteira = await _carteiraRepository.ObterPorIdAsync(carteiraId);
        if (carteira == null)
        {
            return Result<PosicaoConsolidadaResponse>.Failure($"Carteira com ID {carteiraId} não encontrada");
        }

        // Buscar todas as transações da carteira
        var transacoes = await _transacaoRepository.ObterPorCarteiraIdAsync(carteiraId);

        // Agrupar por ativo
        var transacoesPorAtivo = transacoes
            .GroupBy(t => t.AtivoId)
            .ToList();

        var posicoes = new List<PosicaoAtivoResponse>();

        foreach (var grupo in transacoesPorAtivo)
        {
            var ativoId = grupo.Key;
            var transacoesAtivo = grupo.OrderBy(t => t.DataTransacao).ToList();

            var posicao = CalcularPosicaoAtivo(transacoesAtivo);

            // Se ainda tem quantidade, adicionar à lista
            if (posicao.QuantidadeAtual > 0)
            {
                posicoes.Add(posicao);
            }
        }

        // Calcular totais
        var valorTotalInvestido = posicoes.Sum(p => p.ValorInvestido);

        // Agrupar por tipo
        var distribuicao = AgruparPorTipo(posicoes, valorTotalInvestido);

        var response = new PosicaoConsolidadaResponse
        {
            CarteiraId = carteiraId,
            CarteiraNome = carteira.Nome,
            DataCalculo = DateTimeOffset.UtcNow,
            Posicoes = posicoes,
            ValorTotalInvestido = valorTotalInvestido,
            ValorTotalAtual = null, // Será implementado quando houver integração com API de cotações
            LucroTotal = null,
            RentabilidadeTotal = null,
            DistribuicaoPorTipo = distribuicao
        };

        return Result<PosicaoConsolidadaResponse>.Success(response);
    }

    public async Task<Result<PosicaoAtivoResponse>> CalcularPosicaoAtivoAsync(long carteiraId, long ativoId, Guid usuarioId)
    {
        // Verificar ownership
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<PosicaoAtivoResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        // Buscar transações do ativo na carteira
        var transacoes = await _transacaoRepository.ObterPorCarteiraEAtivoAsync(carteiraId, ativoId);

        if (!transacoes.Any())
        {
            return Result<PosicaoAtivoResponse>.Failure("Não há transações para este ativo nesta carteira");
        }

        var transacoesOrdenadas = transacoes.OrderBy(t => t.DataTransacao).ToList();
        var posicao = CalcularPosicaoAtivo(transacoesOrdenadas);

        return Result<PosicaoAtivoResponse>.Success(posicao);
    }

    public async Task<Result<List<PosicaoConsolidadaResponse>>> CalcularTodasPosicoesAsync(Guid usuarioId)
    {
        var carteiras = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);

        var posicoes = new List<PosicaoConsolidadaResponse>();

        foreach (var carteira in carteiras)
        {
            var resultado = await CalcularPosicaoAsync(carteira.Id, usuarioId);
            if (resultado.IsSuccess && resultado.Data != null)
            {
                posicoes.Add(resultado.Data);
            }
        }

        return Result<List<PosicaoConsolidadaResponse>>.Success(posicoes);
    }

    private PosicaoAtivoResponse CalcularPosicaoAtivo(List<Transacao> transacoes)
    {
        if (!transacoes.Any())
        {
            throw new ArgumentException("Lista de transações não pode estar vazia");
        }

        var primeiraTransacao = transacoes.First();
        var ativo = primeiraTransacao.Ativo;

        decimal quantidadeAtual = 0;
        decimal precoMedio = 0;
        decimal dividendosRecebidos = 0;
        DateTimeOffset? dataPrimeiraCompra = null;
        DateTimeOffset? dataUltimaTransacao = null;

        foreach (var transacao in transacoes)
        {
            dataUltimaTransacao = transacao.DataTransacao;

            switch (transacao.TipoTransacao)
            {
                case TipoTransacao.Compra:
                    if (dataPrimeiraCompra == null)
                    {
                        dataPrimeiraCompra = transacao.DataTransacao;
                    }

                    // Weighted Average Cost (WAC)
                    if (quantidadeAtual + transacao.Quantidade > 0)
                    {
                        precoMedio = ((quantidadeAtual * precoMedio) + (transacao.Quantidade * transacao.Preco))
                                   / (quantidadeAtual + transacao.Quantidade);
                    }
                    else
                    {
                        precoMedio = transacao.Preco;
                    }

                    quantidadeAtual += transacao.Quantidade;
                    break;

                case TipoTransacao.Venda:
                    // Venda reduz quantidade mas mantém preço médio
                    quantidadeAtual -= Math.Abs(transacao.Quantidade);

                    // Se zerou a posição, zera o preço médio também
                    if (quantidadeAtual <= 0)
                    {
                        quantidadeAtual = 0;
                        precoMedio = 0;
                    }
                    break;

                case TipoTransacao.Dividendo:
                case TipoTransacao.JCP:
                    // Dividendo: Quantidade armazena o valor total recebido
                    dividendosRecebidos += transacao.Preco * Math.Abs(transacao.Quantidade);
                    break;

                case TipoTransacao.Bonus:
                    // Bonificação aumenta quantidade sem custo
                    quantidadeAtual += transacao.Quantidade;

                    // Recalcula preço médio (custo total / nova quantidade)
                    if (quantidadeAtual > 0)
                    {
                        var custoTotal = (quantidadeAtual - transacao.Quantidade) * precoMedio;
                        precoMedio = custoTotal / quantidadeAtual;
                    }
                    break;

                case TipoTransacao.Split:
                    // Split: Quantidade representa o fator de multiplicação
                    // Ex: Split 2:1 -> Quantidade = 2
                    var fatorSplit = transacao.Quantidade;
                    quantidadeAtual *= fatorSplit;
                    precoMedio /= fatorSplit;
                    break;

                case TipoTransacao.Grupamento:
                    // Grupamento: Quantidade representa o fator de divisão
                    // Ex: Grupamento 10:1 -> Quantidade = 10
                    var fatorGrupamento = transacao.Quantidade;
                    quantidadeAtual /= fatorGrupamento;
                    precoMedio *= fatorGrupamento;
                    break;
            }
        }

        var valorInvestido = quantidadeAtual * precoMedio;

        return new PosicaoAtivoResponse
        {
            AtivoId = ativo?.Id ?? 0,
            AtivoNome = ativo?.Nome ?? string.Empty,
            AtivoCodigo = ativo?.Codigo ?? string.Empty,
            AtivoTipo = ativo?.Tipo ?? string.Empty,
            QuantidadeAtual = quantidadeAtual,
            PrecoMedio = precoMedio,
            ValorInvestido = valorInvestido,
            PrecoAtual = null,
            ValorAtual = null,
            Lucro = null,
            Rentabilidade = null,
            DividendosRecebidos = dividendosRecebidos,
            DataPrimeiraCompra = dataPrimeiraCompra,
            DataUltimaTransacao = dataUltimaTransacao
        };
    }

    private List<DistribuicaoTipoResponse> AgruparPorTipo(
        List<PosicaoAtivoResponse> posicoes,
        decimal valorTotalInvestido)
    {
        if (valorTotalInvestido == 0)
        {
            return new List<DistribuicaoTipoResponse>();
        }

        var distribuicao = posicoes
            .GroupBy(p => p.AtivoTipo)
            .Select(g => new DistribuicaoTipoResponse
            {
                Tipo = g.Key,
                ValorInvestido = g.Sum(p => p.ValorInvestido),
                Percentual = (g.Sum(p => p.ValorInvestido) / valorTotalInvestido) * 100,
                QuantidadeAtivos = g.Count()
            })
            .OrderByDescending(d => d.ValorInvestido)
            .ToList();

        return distribuicao;
    }
}
