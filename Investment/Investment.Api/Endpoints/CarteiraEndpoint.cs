using Gridify;
using Investment.Application.DTOs.Carteira;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class CarteiraEndpoint
{
    public static void RegistrarCarteiraEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/carteiras")
            .WithName("Carteiras")
            .WithTags("Carteiras")
            .RequireAuthorization();

        // GET /api/v1/carteiras - Listar carteiras do usuário com paginação e filtros (com informações de posição)
        group.MapGet("", async ([AsParameters] GridifyQuery query, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterComPosicaoAsync(query, usuarioId);

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
        .WithName("Listar Carteiras com Posição")
        .WithDescription("Lista todas as carteiras do usuário autenticado com informações de valor investido e rentabilidade, com suporte a paginação e filtros (Gridify)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/carteiras/{id} - Obter carteira por ID com informações de posição
        group.MapGet("/{id:long}", async (long id, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterComPosicaoPorIdAsync(id, usuarioId);

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
        .WithName("Obter Carteira por ID")
        .WithDescription("Obtém uma carteira específica do usuário com informações de valor total e rentabilidade")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/carteiras/{id}/detalhes - Obter carteira com detalhes
        group.MapGet("/{id:long}/detalhes", async (long id, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterComDetalhesAsync(id, usuarioId);

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
        .WithName("Obter Carteira com Detalhes")
        .WithDescription("Obtém uma carteira com todos os ativos e transações")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/carteiras - Criar nova carteira
        group.MapPost("", async (CarteiraRequest request, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.CriarAsync(request, usuarioId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Created($"/api/v1/carteiras/{resultado.Data!.Id}", resultado.Data);
        })
        .WithName("Criar Carteira")
        .WithDescription("Cria uma nova carteira para o usuário autenticado")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PUT /api/v1/carteiras/{id} - Atualizar carteira
        group.MapPut("/{id:long}", async (long id, CarteiraRequest request, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.AtualizarAsync(id, request, usuarioId);

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
        .WithName("Atualizar Carteira")
        .WithDescription("Atualiza uma carteira existente")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // DELETE /api/v1/carteiras/{id} - Excluir carteira
        group.MapDelete("/{id:long}", async (long id, HttpContext context, ICarteiraService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ExcluirAsync(id, usuarioId);

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
        .WithName("Excluir Carteira")
        .WithDescription("Exclui uma carteira (não permitido se houver transações)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/carteiras/{id}/ativos - Obter posições (ativos) da carteira
        group.MapGet("/{id:long}/ativos", async (long id, HttpContext context, IPosicaoService posicaoService) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await posicaoService.CalcularPosicaoAsync(id, usuarioId);

            if (!resultado.IsSuccess)
            {
                return Results.NotFound(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            // Retornar apenas a lista de posições (ativos)
            return Results.Ok(resultado.Data.Posicoes);
        })
        .WithName("Listar Ativos da Carteira")
        .WithDescription("Obtém a lista de ativos (posições) da carteira com informações de quantidade, preço médio e rentabilidade")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
