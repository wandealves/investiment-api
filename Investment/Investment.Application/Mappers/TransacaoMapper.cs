using Investment.Application.DTOs.Transacao;
using Investment.Domain.Entidades;

namespace Investment.Application.Mappers;

public static class TransacaoMapper
{
    public static Transacao ToEntity(TransacaoRequest request)
    {
        return new Transacao
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            AtivoId = request.AtivoId,
            Quantidade = request.Quantidade,
            Preco = request.Preco,
            TipoTransacao = request.TipoTransacao,
            DataTransacao = request.DataTransacao.ToUniversalTime()
        };
    }

    public static TransacaoResponse ToResponse(Transacao transacao)
    {
        return new TransacaoResponse
        {
            Id = transacao.Id,
            CarteiraId = transacao.CarteiraId,
            AtivoId = transacao.AtivoId,
            AtivoNome = transacao.Ativo?.Nome ?? string.Empty,
            AtivoCodigo = transacao.Ativo?.Codigo ?? string.Empty,
            Quantidade = transacao.Quantidade,
            Preco = transacao.Preco,
            ValorTotal = transacao.Quantidade * transacao.Preco,
            TipoTransacao = transacao.TipoTransacao,
            DataTransacao = transacao.DataTransacao
        };
    }

    public static List<TransacaoResponse> ToResponseList(List<Transacao> transacoes)
    {
        return transacoes.Select(ToResponse).ToList();
    }

    public static TransacaoComDetalhesResponse ToResponseComDetalhes(Transacao transacao)
    {
        var response = new TransacaoComDetalhesResponse
        {
            Id = transacao.Id,
            CarteiraId = transacao.CarteiraId,
            AtivoId = transacao.AtivoId,
            AtivoNome = transacao.Ativo?.Nome ?? string.Empty,
            AtivoCodigo = transacao.Ativo?.Codigo ?? string.Empty,
            Quantidade = transacao.Quantidade,
            Preco = transacao.Preco,
            ValorTotal = transacao.Quantidade * transacao.Preco,
            TipoTransacao = transacao.TipoTransacao,
            DataTransacao = transacao.DataTransacao,
            Carteira = transacao.Carteira != null ? CarteiraMapper.ToResponse(transacao.Carteira) : default!,
            Ativo = transacao.Ativo != null ? AtivoMapper.ToResponse(transacao.Ativo) : default!
        };

        return response;
    }

    public static void UpdateEntity(Transacao transacao, TransacaoRequest request)
    {
        transacao.CarteiraId = request.CarteiraId;
        transacao.AtivoId = request.AtivoId;
        transacao.Quantidade = request.Quantidade;
        transacao.Preco = request.Preco;
        transacao.TipoTransacao = request.TipoTransacao;
        transacao.DataTransacao = request.DataTransacao.ToUniversalTime();
    }
}
