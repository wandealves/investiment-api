using Investment.Application.DTOs.Usuario;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IUsuarioService
{
    Task<Result<UsuarioResponse>> ObterPorIdAsync(Guid id, Guid usuarioAutenticadoId);
    Task<Result<UsuarioComCarteirasResponse>> ObterComCarteirasAsync(Guid id, Guid usuarioAutenticadoId);
    Task<Result<UsuarioResponse>> AtualizarAsync(Guid id, UsuarioRequest request, Guid usuarioAutenticadoId);
    Task<Result> ExcluirAsync(Guid id, Guid usuarioAutenticadoId);
}
