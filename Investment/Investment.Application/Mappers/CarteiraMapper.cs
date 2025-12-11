using Investment.Application.DTOs.Carteira;
using Investment.Domain.Entidades;

namespace Investment.Application.Mappers;

public static class CarteiraMapper
{
    public static Carteira ToEntity(CarteiraRequest request, Guid usuarioId)
    {
        return new Carteira
        {
            UsuarioId = usuarioId,
            Nome = request.Nome.Trim(),
            CriadaEm = DateTimeOffset.UtcNow
        };
    }

    public static CarteiraResponse ToResponse(Carteira carteira)
    {
        return new CarteiraResponse
        {
            Id = carteira.Id,
            UsuarioId = carteira.UsuarioId,
            Nome = carteira.Nome,
            Descricao = null, // Campo ainda não existe na entidade
            CriadaEm = carteira.CriadaEm,
            TotalAtivos = carteira.CarteirasAtivos?.Count ?? 0,
            TotalTransacoes = carteira.Transacoes?.Count ?? 0
        };
    }

    public static List<CarteiraResponse> ToResponseList(List<Carteira> carteiras)
    {
        return carteiras.Select(ToResponse).ToList();
    }

    public static CarteiraComDetalhesResponse ToResponseComDetalhes(Carteira carteira)
    {
        var response = new CarteiraComDetalhesResponse
        {
            Id = carteira.Id,
            UsuarioId = carteira.UsuarioId,
            Nome = carteira.Nome,
            Descricao = null,
            CriadaEm = carteira.CriadaEm,
            TotalAtivos = carteira.CarteirasAtivos?.Count ?? 0,
            TotalTransacoes = carteira.Transacoes?.Count ?? 0,
            Ativos = carteira.CarteirasAtivos?
                .Select(ca => AtivoMapper.ToResponse(ca.Ativo))
                .ToList() ?? new(),
            Transacoes = carteira.Transacoes?
                .Select(TransacaoMapper.ToResponse)
                .ToList() ?? new()
        };

        return response;
    }

    public static void UpdateEntity(Carteira carteira, CarteiraRequest request)
    {
        carteira.Nome = request.Nome.Trim();
    }
}
