using Investment.Application.DTOs.Carteira;

namespace Investment.Application.DTOs.Usuario;

public class UsuarioComCarteirasResponse : UsuarioResponse
{
    public List<CarteiraResponse> Carteiras { get; set; } = new();
}
