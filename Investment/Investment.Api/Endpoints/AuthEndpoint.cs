using Investment.Application.DTOs.Auth;
using Investment.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Investment.Api.Endpoints;

public static class AuthEndpoint
{
    public static void RegistrarAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth")
            .WithName("Autenticação")
            .WithTags("Autenticação");

        // Helper para criar CookieOptions baseado no ambiente
        static CookieOptions GetCookieOptions(HttpContext context)
        {
            var isDevelopment = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

            // Em desenvolvimento: Secure = false para permitir HTTP
            // Em produção: Secure = true para exigir HTTPS
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,  // false em Development, true em Production
                SameSite = SameSiteMode.Lax,  // Lax permite cookies em navegação cross-site
                Expires = DateTimeOffset.UtcNow.AddHours(24),
                Path = "/"
            };
        }

        // POST /api/v1/auth/register - Registro de novo usuário (público)
        group.MapPost("/register", async (RegisterRequest request, IAuthService service, HttpContext context) =>
        {
            var resultado = await service.RegisterAsync(request);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            // Configurar cookie httpOnly com o token JWT
            context.Response.Cookies.Append("access_token", resultado.Data!.Token, GetCookieOptions(context));

            // Retornar apenas dados do usuário (sem token no body)
            return Results.Created($"/api/v1/usuarios/{resultado.Data.Usuario.Id}", new
            {
                usuario = resultado.Data.Usuario
            });
        })
        .WithName("Registrar Usuário")
        .WithDescription("Registra um novo usuário no sistema")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // POST /api/v1/auth/login - Login de usuário (público)
        group.MapPost("/login", async (LoginRequest request, IAuthService service, HttpContext context) =>
        {
            var resultado = await service.LoginAsync(request);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            // Configurar cookie httpOnly com o token JWT
            context.Response.Cookies.Append("access_token", resultado.Data!.Token, GetCookieOptions(context));

            // Retornar apenas dados do usuário (sem token no body)
            return Results.Ok(new
            {
                usuario = resultado.Data.Usuario
            });
        })
        .WithName("Login")
        .WithDescription("Autentica um usuário e retorna um token JWT via cookie httpOnly")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // POST /api/v1/auth/alterar-senha - Alterar senha (protegido)
        group.MapPost("/alterar-senha", async (
            AlterarSenhaRequest request,
            HttpContext context,
            IAuthService service) =>
        {
            var usuarioId = context.GetUsuarioId();

            var resultado = await service.AlterarSenhaAsync(
                usuarioId,
                request.SenhaAtual,
                request.NovaSenha);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Alterar Senha")
        .WithDescription("Altera a senha do usuário autenticado")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/auth/me - Obter dados do usuário autenticado (protegido)
        group.MapGet("/me", async (
            HttpContext context,
            IUsuarioService usuarioService) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await usuarioService.ObterPorIdAsync(usuarioId, usuarioId);

            if (!resultado.IsSuccess)
            {
                return Results.NotFound(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(resultado.Data);
        })
        .RequireAuthorization()
        .WithName("Usuário Autenticado")
        .WithDescription("Retorna os dados do usuário autenticado")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/auth/logout - Logout (limpa cookie)
        group.MapPost("/logout", (HttpContext context) =>
        {
            // Remover cookie do token (mesmas opções do cookie original)
            context.Response.Cookies.Delete("access_token", GetCookieOptions(context));

            return Results.Ok(new { message = "Logout realizado com sucesso" });
        })
        .WithName("Logout")
        .WithDescription("Remove o cookie de autenticação")
        .Produces<object>(StatusCodes.Status200OK);
    }
}

// DTO para alterar senha
public class AlterarSenhaRequest
{
    public string SenhaAtual { get; set; } = default!;
    public string NovaSenha { get; set; } = default!;
}

// Extension method para obter usuário autenticado
public static class HttpContextExtensions
{
    public static Guid GetUsuarioId(this HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }
        return Guid.Parse(userIdClaim);
    }
}
