using Investment.Application.DTOs.Importacao;
using Investment.Application.DTOs.Transacao;
using Investment.Application.Mappers;
using Investment.Application.Services.PDF;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class ImportacaoService : IImportacaoService
{
    private readonly IPdfParserService _pdfParserService;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IAtivoRepository _ativoRepository;

    public ImportacaoService(
        IPdfParserService pdfParserService,
        ITransacaoRepository transacaoRepository,
        ICarteiraRepository carteiraRepository,
        IAtivoRepository ativoRepository)
    {
        _pdfParserService = pdfParserService;
        _transacaoRepository = transacaoRepository;
        _carteiraRepository = carteiraRepository;
        _ativoRepository = ativoRepository;
    }

    public async Task<Result<ImportacaoResponse>> PreviewImportacaoAsync(
        long carteiraId, Stream pdfStream, string corretoraTipo, Guid usuarioId)
    {
        return await ProcessarImportacaoAsync(carteiraId, pdfStream, corretoraTipo, usuarioId, preview: true);
    }

    public async Task<Result<ImportacaoResponse>> ImportarNotaAsync(
        long carteiraId, Stream pdfStream, string corretoraTipo, Guid usuarioId)
    {
        return await ProcessarImportacaoAsync(carteiraId, pdfStream, corretoraTipo, usuarioId, preview: false);
    }

    private async Task<Result<ImportacaoResponse>> ProcessarImportacaoAsync(
        long carteiraId, Stream pdfStream, string corretoraTipo, Guid usuarioId, bool preview)
    {
        var response = new ImportacaoResponse { Sucesso = false };

        // Verificar ownership da carteira
        var pertenceAoUsuario = await _carteiraRepository.UsuarioPossuiCarteiraAsync(usuarioId, carteiraId);
        if (!pertenceAoUsuario)
        {
            return Result<ImportacaoResponse>.Failure("Acesso negado: esta carteira não pertence ao usuário autenticado");
        }

        // Parse do PDF
        var resultadoParse = await _pdfParserService.ParseNotaAsync(pdfStream, corretoraTipo);
        if (!resultadoParse.IsSuccess || resultadoParse.Data == null)
        {
            return Result<ImportacaoResponse>.Failure(resultadoParse.Errors);
        }

        var nota = resultadoParse.Data;

        // Processar cada operação
        var transacoesPreview = new List<TransacaoResponse>();
        var transacoesCriadas = 0;
        var transacoesIgnoradas = 0;

        foreach (var operacao in nota.Operacoes)
        {
            try
            {
                // Buscar ou criar ativo
                var ativo = await ObterOuCriarAtivoAsync(operacao.Ticker, response);
                if (ativo == null)
                {
                    transacoesIgnoradas++;
                    continue;
                }

                // Verificar duplicata
                if (await VerificarDuplicataAsync(carteiraId, ativo.Id, nota.DataPregao))
                {
                    response.Avisos.Add($"Transação para {operacao.Ticker} na data {nota.DataPregao:dd/MM/yyyy} pode ser duplicada (existe transação ±1 dia)");
                    if (!preview)
                    {
                        transacoesIgnoradas++;
                        continue;
                    }
                }

                // Calcular preço final com custos proporcionais
                var precoFinal = operacao.PrecoUnitario + (operacao.TaxasProporcionais / operacao.Quantidade);

                // Criar transação
                var transacao = new Transacao
                {
                    Id = Guid.NewGuid(),
                    CarteiraId = carteiraId,
                    AtivoId = ativo.Id,
                    Quantidade = operacao.TipoOperacao == "V" ? -Math.Abs(operacao.Quantidade) : operacao.Quantidade,
                    Preco = precoFinal,
                    TipoTransacao = operacao.TipoOperacao == "C" ? TipoTransacao.Compra : TipoTransacao.Venda,
                    DataTransacao = nota.DataPregao.ToUniversalTime()
                };

                if (!preview)
                {
                    // Salvar no banco
                    await _transacaoRepository.SalvarAsync(transacao);
                    transacoesCriadas++;
                }

                // Adicionar ao preview
                transacao.Ativo = ativo; // Para o mapper funcionar
                transacoesPreview.Add(TransacaoMapper.ToResponse(transacao));
            }
            catch (Exception ex)
            {
                response.Erros.Add($"Erro ao processar operação {operacao.Ticker}: {ex.Message}");
                transacoesIgnoradas++;
            }
        }

        response.Sucesso = response.Erros.Count == 0;
        response.TransacoesCriadas = preview ? 0 : transacoesCriadas;
        response.TransacoesIgnoradas = transacoesIgnoradas;
        response.PreviewTransacoes = transacoesPreview;

        if (!preview && transacoesCriadas == 0 && transacoesIgnoradas == 0)
        {
            response.Avisos.Add("Nenhuma transação foi importada. Verifique o PDF e tente novamente.");
        }

        return Result<ImportacaoResponse>.Success(response);
    }

    private async Task<Ativo?> ObterOuCriarAtivoAsync(string ticker, ImportacaoResponse response)
    {
        // Buscar ativo existente
        var ativo = await _ativoRepository.ObterPorCodigoAsync(ticker);

        if (ativo != null)
            return ativo;

        // Auto-criar ativo com tipo "Acao" (usuário pode editar depois)
        try
        {
            ativo = new Ativo
            {
                Codigo = ticker,
                Nome = ticker, // Nome provisório
                Tipo = "Acao" // Tipo padrão
            };

            await _ativoRepository.SalvarAsync(ativo);
            response.Avisos.Add($"Ativo '{ticker}' criado automaticamente como 'Acao'. Edite se necessário.");

            return ativo;
        }
        catch (Exception ex)
        {
            response.Erros.Add($"Erro ao criar ativo '{ticker}': {ex.Message}");
            return null;
        }
    }

    private async Task<bool> VerificarDuplicataAsync(long carteiraId, long ativoId, DateTimeOffset dataPregao)
    {
        // Verificar se existe transação para o mesmo ativo e carteira em ±1 dia
        var dataInicio = dataPregao.AddDays(-1);
        var dataFim = dataPregao.AddDays(1);

        var transacoesExistentes = await _transacaoRepository.ObterPorCarteiraEAtivoAsync(carteiraId, ativoId);

        return transacoesExistentes.Any(t =>
            t.DataTransacao >= dataInicio && t.DataTransacao <= dataFim);
    }
}
