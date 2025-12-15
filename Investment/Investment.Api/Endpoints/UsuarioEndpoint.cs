using Investment.Application.DTOs.Usuario;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class UsuarioEndpoint
{
    public static void RegistrarUsuarioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/usuarios")
            .WithName("Usuários")
            .WithTags("Usuários")
            .RequireAuthorization();

        // GET /api/v1/usuarios/{id} - Obter usuário por ID
        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, IUsuarioService service) =>
        {
            var usuarioAutenticadoId = context.GetUsuarioId();
            var resultado = await service.ObterPorIdAsync(id, usuarioAutenticadoId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(resultado.Data);
        })
        .WithName("Obter Usuário por ID")
        .WithDescription("Obtém os dados do usuário autenticado")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/usuarios/{id}/carteiras - Obter usuário com carteiras
        group.MapGet("/{id:guid}/carteiras", async (Guid id, HttpContext context, IUsuarioService service) =>
        {
            var usuarioAutenticadoId = context.GetUsuarioId();
            var resultado = await service.ObterComCarteirasAsync(id, usuarioAutenticadoId);

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
        .WithName("Obter Usuário com Carteiras")
        .WithDescription("Obtém os dados do usuário com todas as suas carteiras")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PUT /api/v1/usuarios/{id} - Atualizar usuário
        group.MapPut("/{id:guid}", async (Guid id, UsuarioRequest request, HttpContext context, IUsuarioService service) =>
        {
            var usuarioAutenticadoId = context.GetUsuarioId();
            var resultado = await service.AtualizarAsync(id, request, usuarioAutenticadoId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(resultado.Data);
        })
        .WithName("Atualizar Usuário")
        .WithDescription("Atualiza os dados do usuário autenticado (nome e email)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // DELETE /api/v1/usuarios/{id} - Excluir usuário
        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, IUsuarioService service) =>
        {
            var usuarioAutenticadoId = context.GetUsuarioId();
            var resultado = await service.ExcluirAsync(id, usuarioAutenticadoId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.NoContent();
        })
        .WithName("Excluir Usuário")
        .WithDescription("Exclui a conta do usuário autenticado")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
