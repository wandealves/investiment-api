using Investment.Application.DTOs.Provento;
using Investment.Domain.Common;
using Investment.Domain.Entidades;

namespace Investment.Application.Mappers;

public static class ProventoMapper
{
    public static Provento ToEntity(ProventoRequest request)
    {
        return new Provento
        {
            AtivoId = request.AtivoId,
            TipoProvento = request.TipoProvento,
            ValorPorCota = request.ValorPorCota,
            DataCom = request.DataCom.ToUniversalTime(),
            DataEx = request.DataEx?.ToUniversalTime(),
            DataPagamento = request.DataPagamento.ToUniversalTime(),
            Status = request.Status,
            Observacao = request.Observacao,
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    public static ProventoResponse ToResponse(Provento provento)
    {
        return new ProventoResponse
        {
            Id = provento.Id,
            AtivoId = provento.AtivoId,
            AtivoNome = provento.Ativo?.Nome ?? string.Empty,
            AtivoCodigo = provento.Ativo?.Codigo ?? string.Empty,
            TipoProvento = provento.TipoProvento,
            TipoProventoDescricao = GetTipoProventoDescricao(provento.TipoProvento),
            ValorPorCota = provento.ValorPorCota,
            DataCom = provento.DataCom,
            DataEx = provento.DataEx,
            DataPagamento = provento.DataPagamento,
            Status = provento.Status,
            StatusDescricao = GetStatusProventoDescricao(provento.Status),
            Observacao = provento.Observacao,
            CriadoEm = provento.CriadoEm
        };
    }

    public static List<ProventoResponse> ToResponseList(List<Provento> proventos)
    {
        return proventos.Select(ToResponse).ToList();
    }

    public static ProventoComTransacoesResponse ToResponseComTransacoes(Provento provento)
    {
        var response = new ProventoComTransacoesResponse
        {
            Id = provento.Id,
            AtivoId = provento.AtivoId,
            AtivoNome = provento.Ativo?.Nome ?? string.Empty,
            AtivoCodigo = provento.Ativo?.Codigo ?? string.Empty,
            TipoProvento = provento.TipoProvento,
            TipoProventoDescricao = GetTipoProventoDescricao(provento.TipoProvento),
            ValorPorCota = provento.ValorPorCota,
            DataCom = provento.DataCom,
            DataEx = provento.DataEx,
            DataPagamento = provento.DataPagamento,
            Status = provento.Status,
            StatusDescricao = GetStatusProventoDescricao(provento.Status),
            Observacao = provento.Observacao,
            CriadoEm = provento.CriadoEm,
            Ativo = provento.Ativo != null ? AtivoMapper.ToResponse(provento.Ativo) : default!,
            Transacoes = provento.Transacoes?.Select(TransacaoMapper.ToResponse).ToList() ?? new List<DTOs.Transacao.TransacaoResponse>(),
            ValorTotalPago = provento.Transacoes?.Sum(t => t.Quantidade * t.Preco) ?? 0
        };

        return response;
    }

    public static void UpdateEntity(Provento provento, ProventoRequest request)
    {
        provento.AtivoId = request.AtivoId;
        provento.TipoProvento = request.TipoProvento;
        provento.ValorPorCota = request.ValorPorCota;
        provento.DataCom = request.DataCom.ToUniversalTime();
        provento.DataEx = request.DataEx?.ToUniversalTime();
        provento.DataPagamento = request.DataPagamento.ToUniversalTime();
        provento.Status = request.Status;
        provento.Observacao = request.Observacao;
    }

    private static string GetTipoProventoDescricao(TipoProvento tipo)
    {
        return tipo switch
        {
            TipoProvento.Dividendo => "Dividendo",
            TipoProvento.JCP => "Juros sobre Capital Próprio",
            TipoProvento.RendimentoFII => "Rendimento de FII",
            TipoProvento.Bonificacao => "Bonificação",
            _ => tipo.ToString()
        };
    }

    private static string GetStatusProventoDescricao(StatusProvento status)
    {
        return status switch
        {
            StatusProvento.Agendado => "Agendado",
            StatusProvento.Pago => "Pago",
            StatusProvento.Cancelado => "Cancelado",
            _ => status.ToString()
        };
    }
}
