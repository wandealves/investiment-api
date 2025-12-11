using Investment.Application.DTOs.Auth;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result> AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha);
}
