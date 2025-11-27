using Investment.Application.DTOs;
using Investment.Domain.Entidades;

namespace Investment.Application.Mappers;

public static class AtivoMapper
{
    public static Ativo ToEntity(AtivoRequest request)
    {
        return new Ativo
        {
            Nome = request.Nome,
            Codigo = request.Codigo.ToUpperInvariant(),
            Tipo = request.Tipo,
            Descricao = request.Descricao
        };
    }
    
    public static AtivoResponse ToResponse(Ativo ativo)
    {
        return new AtivoResponse
        {
            Id = ativo.Id,
            Nome = ativo.Nome,
            Codigo = ativo.Codigo,
            Tipo = ativo.Tipo,
            Descricao = ativo.Descricao
        };
    }
    
    public static List<AtivoResponse> ToResponseList(List<Ativo> ativos)
    {
        return ativos.Select(ToResponse).ToList();
    }
    
    public static void UpdateEntity(Ativo ativo, AtivoRequest request)
    {
        ativo.Nome = request.Nome;
        ativo.Codigo = request.Codigo.ToUpperInvariant();
        ativo.Tipo = request.Tipo;
        ativo.Descricao = request.Descricao;
    }
}
