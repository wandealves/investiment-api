using Investment.Application.DTOs.ImpostoRenda;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class ImpostoRendaService : IImpostoRendaService
{
    private readonly ICalculoIRRepository _calculoIRRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public ImpostoRendaService(
        ITransacaoRepository transacaoRepository,
        ICarteiraRepository carteiraRepository,
        ICalculoIRRepository calculoIRRepository)
    {
        _transacaoRepository = transacaoRepository;
        _carteiraRepository = carteiraRepository;
        _calculoIRRepository = calculoIRRepository;
    }

    public async Task<Result<CalculoIRResponse>> CalcularIRAsync(long carteiraId, int? ano, Guid usuarioId)
    {
        var transacoes =
            await _transacaoRepository.ObterPorCarteiraEAnoAsync(carteiraId,
                ano != null ? ano.Value : DateTime.Now.Year);

        if (!transacoes.Any())
            return Result<CalculoIRResponse>.Failure(
                $"Não há transações para {(ano.HasValue ? $"o ano {ano.Value}" : "calcular")}");

        var grupoTransacaoAnoMesDia =
            transacoes.GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month, t.DataTransacao.Day });

        var itensCalculo = new List<ItemCalculoIR>();
        foreach (var grupo in grupoTransacaoAnoMesDia)
        {
            var trans = grupo.ToList();
            foreach (var transacao in trans)
            {
                var totalAtivo = transacao.Preco * transacao.Quantidade;

                itensCalculo.Add(new ItemCalculoIR
                {
                    Id = Guid.NewGuid(),
                    AtivoId = transacao.AtivoId,
                    Quantidade = transacao.Quantidade,
                    Total = totalAtivo,
                    PrecoAtual = transacao.Preco,
                    PrecoMedio = 0,
                    Rendimento = null,
                    Data = transacao.DataTransacao
                });
            }
        }

        foreach (var itemCalculoIR in itensCalculo.OrderBy(i => i.Data))
        {
            var totalInvestido = itensCalculo.FindAll(t => t.Data == itemCalculoIR.Data).Sum(i => i.Total);
            var totalTaxa = transacoes.FirstOrDefault(t => t.DataTransacao == itemCalculoIR.Data)?.Taxa ?? 0;
            var porcetagem = itemCalculoIR.Total / totalInvestido;
            var taxaRateada = porcetagem * totalTaxa;
            itemCalculoIR.TaxasRateadas = taxaRateada;
            itemCalculoIR.Total = itemCalculoIR.Total + taxaRateada;
        }

        // Calcular totais consolidados
        var valorTotalInvestido = itensCalculo.Sum(i => i.Total);
        var totalTaxasRateadas = itensCalculo.Sum(i => i.TaxasRateadas);

        // Verificar se já existe cálculo para este usuário e ano
        var calculoExistente = await _calculoIRRepository.ObterUltimoPorUsuarioEAnoAsync(usuarioId, ano);

        if (calculoExistente != null)
            // Excluir cálculo antigo para fazer atualização
            await _calculoIRRepository.ExcluirAsync(calculoExistente.Id);

        // Criar novo cálculo
        var calculoIR = new CalculoIR
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            CarteiraId = carteiraId,
            Ano = ano,
            Data = DateTimeOffset.UtcNow,
            Total = valorTotalInvestido,
            Valor = null,
            TotalTaxas = totalTaxasRateadas,
            Itens = itensCalculo
        };

        // Salvar no banco
        await _calculoIRRepository.SalvarAsync(calculoIR);

        // Recarregar do banco com propriedades de navegação (Ativo)
        var calculoSalvo = await _calculoIRRepository.ObterPorIdAsync(calculoIR.Id);
        if (calculoSalvo == null)
            return Result<CalculoIRResponse>.Failure("Erro ao salvar cálculo de IR");

        // Mapear para DTO
        var itens = calculoSalvo.Itens.OrderBy(it => it.Data).Select(i => new ItemCalculoIRResponse
        {
            AtivoId = i.AtivoId,
            AtivoNome = i.Ativo.Nome,
            AtivoCodigo = i.Ativo.Codigo,
            AtivoTipo = i.Ativo.Tipo,
            Quantidade = i.Quantidade,
            Total = i.Total,
            PrecoMedio = i.PrecoMedio,
            PrecoAtual = i.PrecoAtual,
            Rendimento = i.Rendimento,
            TaxasRateadas = i.TaxasRateadas,
            HistoricoCompras = new List<HistoricoCompraResponse>(),
            Data = i.Data ?? new DateTimeOffset()
        }).ToList();
        var response = new CalculoIRResponse
        {
            Id = calculoSalvo.Id,
            Ano = calculoSalvo.Ano,
            Data = calculoSalvo.Data,
            Total = calculoSalvo.Total,
            Valor = calculoSalvo.Valor,
            TotalTaxas = calculoSalvo.TotalTaxas,
            Itens = itens
        };

        return Result<CalculoIRResponse>.Success(response);
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
        return await CalcularIRAsync(calculoExistente.CarteiraId, calculoExistente.Ano, usuarioId);
    }

    public async Task<Result<List<CalculoIRResponse>>> ObterHistoricoCalculosAsync(Guid usuarioId)
    {
        var calculos = await _calculoIRRepository.ObterPorUsuarioIdAsync(usuarioId);

        var response = calculos.Select(c => new CalculoIRResponse
        {
            Id = c.Id,
            Ano = c.Ano,
            Data = c.Data,
            Total = c.Total,
            Valor = c.Valor,
            TotalTaxas = c.TotalTaxas,
            Itens = c.Itens.Select(i => new ItemCalculoIRResponse
            {
                AtivoId = i.AtivoId,
                AtivoNome = i.Ativo.Nome,
                AtivoCodigo = i.Ativo.Codigo,
                AtivoTipo = i.Ativo.Tipo,
                Quantidade = i.Quantidade,
                Total = i.Total,
                PrecoMedio = i.PrecoMedio,
                PrecoAtual = i.PrecoAtual,
                Rendimento = i.Rendimento,
                TaxasRateadas = i.TaxasRateadas,
                Data = i.Data ?? new DateTimeOffset(),
                HistoricoCompras = new List<HistoricoCompraResponse>()
            }).OrderBy(it => it.Data).ToList()
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