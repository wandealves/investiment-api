using Investment.Application.DTOs.Cotacao;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Investment.Application.Services.Cotacao;

public class CotacaoService : ICotacaoService
{
    private readonly IAtivoRepository _ativoRepository;
    private readonly ICotacaoRepository _cotacaoRepository;
    private readonly ICotacaoProviderStrategy _provider;
    private readonly ILogger<CotacaoService> _logger;

    public CotacaoService(
        IAtivoRepository ativoRepository,
        ICotacaoRepository cotacaoRepository,
        ICotacaoProviderStrategy provider,
        ILogger<CotacaoService> logger)
    {
        _ativoRepository = ativoRepository;
        _cotacaoRepository = cotacaoRepository;
        _provider = provider;
        _logger = logger;
    }

    public async Task AtualizarTodasCotacoesAsync()
    {
        _logger.LogInformation("Iniciando atualização de cotações às {DataHora}",
            DateTimeOffset.UtcNow);

        var ativos = await _ativoRepository.ObterTodosAsync();
        var codigos = ativos.Select(a => a.Codigo).ToList();

        _logger.LogInformation("Atualizando {Total} ativos", codigos.Count);

        // Buscar cotações em lote
        var cotacoesDto = await _provider.ObterCotacoesEmLoteAsync(codigos);

        var sucessos = 0;
        var falhas = 0;

        foreach (var cotacaoDto in cotacoesDto)
        {
            try
            {
                var ativo = ativos.FirstOrDefault(a => a.Codigo == cotacaoDto.Codigo);
                if (ativo == null)
                {
                    _logger.LogWarning("Ativo {Codigo} não encontrado no banco", cotacaoDto.Codigo);
                    continue;
                }

                // 1. Salvar no histórico (tabela Cotacoes)
                var cotacao = new Domain.Entidades.Cotacao
                {
                    AtivoId = ativo.Id,
                    Preco = cotacaoDto.Preco,
                    DataHora = cotacaoDto.DataHora,
                    TipoCotacao = "Fechamento",
                    PrecoAbertura = cotacaoDto.PrecoAbertura,
                    PrecoMaximo = cotacaoDto.PrecoMaximo,
                    PrecoMinimo = cotacaoDto.PrecoMinimo,
                    Volume = cotacaoDto.Volume,
                    Fonte = _provider.NomeProvedor
                };

                await _cotacaoRepository.SalvarAsync(cotacao);

                // 2. Atualizar cache no Ativo
                ativo.PrecoAtual = cotacaoDto.Preco;
                ativo.PrecoAtualizadoEm = cotacaoDto.DataHora;
                ativo.FonteCotacao = _provider.NomeProvedor;

                await _ativoRepository.AtualizarAsync(ativo);

                sucessos++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar cotação de {Codigo}", cotacaoDto.Codigo);
                falhas++;
            }
        }

        _logger.LogInformation(
            "Atualização concluída: {Sucessos} sucessos, {Falhas} falhas",
            sucessos, falhas);
    }

    public async Task AtualizarCotacaoAtivoAsync(long ativoId)
    {
        var ativo = await _ativoRepository.ObterPorIdAsync(ativoId);
        if (ativo == null)
            throw new InvalidOperationException($"Ativo {ativoId} não encontrado");

        var cotacaoDto = await _provider.ObterCotacaoAsync(ativo.Codigo);
        if (cotacaoDto == null)
            throw new InvalidOperationException($"Não foi possível obter cotação para {ativo.Codigo}");

        // Salvar histórico
        var cotacao = new Domain.Entidades.Cotacao
        {
            AtivoId = ativo.Id,
            Preco = cotacaoDto.Preco,
            DataHora = cotacaoDto.DataHora,
            TipoCotacao = "Intraday",
            PrecoAbertura = cotacaoDto.PrecoAbertura,
            PrecoMaximo = cotacaoDto.PrecoMaximo,
            PrecoMinimo = cotacaoDto.PrecoMinimo,
            Volume = cotacaoDto.Volume,
            Fonte = _provider.NomeProvedor
        };

        await _cotacaoRepository.SalvarAsync(cotacao);

        // Atualizar cache
        ativo.PrecoAtual = cotacaoDto.Preco;
        ativo.PrecoAtualizadoEm = cotacaoDto.DataHora;
        ativo.FonteCotacao = _provider.NomeProvedor;

        await _ativoRepository.AtualizarAsync(ativo);
    }

    public async Task<Result<List<CotacaoResponse>>> ObterHistoricoAsync(
        long ativoId,
        DateTimeOffset inicio,
        DateTimeOffset fim)
    {
        var ativo = await _ativoRepository.ObterPorIdAsync(ativoId);
        if (ativo == null)
        {
            return Result<List<CotacaoResponse>>.Failure($"Ativo com ID {ativoId} não encontrado");
        }

        var cotacoes = await _cotacaoRepository.ObterPorAtivoEPeriodoAsync(ativoId, inicio, fim);

        var responses = cotacoes.Select(c => new CotacaoResponse
        {
            Id = c.Id,
            AtivoId = c.AtivoId,
            AtivoNome = ativo.Nome,
            AtivoCodigo = ativo.Codigo,
            Preco = c.Preco,
            DataHora = c.DataHora,
            TipoCotacao = c.TipoCotacao,
            PrecoAbertura = c.PrecoAbertura,
            PrecoMaximo = c.PrecoMaximo,
            PrecoMinimo = c.PrecoMinimo,
            Volume = c.Volume,
            Fonte = c.Fonte
        }).ToList();

        return Result<List<CotacaoResponse>>.Success(responses);
    }

    public async Task<decimal?> ObterPrecoAtualAsync(long ativoId)
    {
        var ativo = await _ativoRepository.ObterPorIdAsync(ativoId);
        return ativo?.PrecoAtual;
    }
}
