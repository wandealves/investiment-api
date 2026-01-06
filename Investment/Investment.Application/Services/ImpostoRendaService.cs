using Investment.Application.DTOs.ImpostoRenda;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;

public class ImpostoRendaService(
    ITransacaoRepository transacaoRepository,
    ICalculoIRRepository calculoIRRepository) : IImpostoRendaService
{
    public async Task<Result<CalculoIRResponse>> CalcularIRAsync(long carteiraId, int? ano, Guid usuarioId)
    {
        var transacoes =
            await transacaoRepository.ObterPorCarteiraEAnoAsync(carteiraId,
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
                itensCalculo.Add(new ItemCalculoIR
                {
                    Id = Guid.NewGuid(),
                    AtivoId = transacao.AtivoId,
                    Quantidade = transacao.Quantidade,
                    Total = transacao.ValorTotal,
                    PrecoAtual = transacao.Preco,
                    PrecoMedio = 0,
                    Rendimento = null,
                    Data = transacao.DataTransacao
                });
        }

        foreach (var itemCalculoIR in itensCalculo.OrderBy(i => i.Data))
        {
            var taxaRateada = itemCalculoIR.Porcetagem(itensCalculo) * itemCalculoIR.TotalTaxa(transacoes);
            itemCalculoIR.TaxasRateadas = taxaRateada;
            itemCalculoIR.Total += taxaRateada;
        }

        var valorTotalInvestido = itensCalculo.Sum(i => i.Total);
        var totalTaxasRateadas = itensCalculo.Sum(i => i.TaxasRateadas);
        var calculoExistente = await calculoIRRepository.ObterUltimoPorUsuarioEAnoAsync(usuarioId, ano);

        if (calculoExistente != null)
            await calculoIRRepository.ExcluirAsync(calculoExistente.Id);
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
        await calculoIRRepository.SalvarAsync(calculoIR);
        var calculoSalvo = await calculoIRRepository.ObterPorIdAsync(calculoIR.Id);
        if (calculoSalvo == null)
            return Result<CalculoIRResponse>.Failure("Erro ao salvar cálculo de IR");
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
        var calculoExistente = await calculoIRRepository.ObterPorIdAsync(calculoId);
        if (calculoExistente == null)
            return Result<CalculoIRResponse>.Failure("Cálculo não encontrado");

        if (calculoExistente.UsuarioId != usuarioId)
            return Result<CalculoIRResponse>.Failure("Acesso negado");

        await calculoIRRepository.ExcluirAsync(calculoId);
        return await CalcularIRAsync(calculoExistente.CarteiraId, calculoExistente.Ano, usuarioId);
    }

    public async Task<Result<List<CalculoIRResponse>>> ObterHistoricoCalculosAsync(Guid usuarioId)
    {
        var calculos = await calculoIRRepository.ObterPorUsuarioIdAsync(usuarioId);

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
        var calculo = await calculoIRRepository.ObterPorIdAsync(calculoId);
        if (calculo == null)
            return Result.Failure("Cálculo não encontrado");

        if (calculo.UsuarioId != usuarioId)
            return Result.Failure("Acesso negado");

        await calculoIRRepository.ExcluirAsync(calculoId);
        return Result.Success();
    }
}