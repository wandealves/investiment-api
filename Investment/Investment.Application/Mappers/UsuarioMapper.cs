using Investment.Application.DTOs.Usuario;
using Investment.Domain.Entidades;

namespace Investment.Application.Mappers;

public static class UsuarioMapper
{
    public static UsuarioResponse ToResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            CriadoEm = usuario.CriadoEm
        };
    }

    public static List<UsuarioResponse> ToResponseList(List<Usuario> usuarios)
    {
        return usuarios.Select(ToResponse).ToList();
    }

    public static UsuarioComCarteirasResponse ToResponseComCarteiras(Usuario usuario)
    {
        var response = new UsuarioComCarteirasResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            CriadoEm = usuario.CriadoEm,
            Carteiras = usuario.Carteiras?.Select(CarteiraMapper.ToResponse).ToList() ?? new()
        };

        return response;
    }

    public static void UpdateEntity(Usuario usuario, UsuarioRequest request)
    {
        usuario.Nome = request.Nome.Trim();
        usuario.Email = request.Email.Trim().ToLowerInvariant();
    }
}
