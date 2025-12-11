using System.Security.Claims;
using Investment.Domain.Entidades;

namespace Investment.Application.Services;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
    ClaimsPrincipal? ValidateToken(string token);
}
