using Gridify;
using Investment.Application.DTOs.Transacao;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class TransacaoEndpoint
{
    public static void RegistrarTransacaoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/transacoes")
            .WithName("Transações")
            .WithTags("Transações")
            .RequireAuthorization();

        // GET /api/v1/transacoes/{id} - Obter transação por ID
        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, ITransacaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterPorIdAsync(id, usuarioId);

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
        .WithName("Obter Transação por ID")
        .WithDescription("Obtém uma transação específica")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/transacoes - Criar nova transação
        group.MapPost("", async (TransacaoRequest request, HttpContext context, ITransacaoService service) =>
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

            return Results.Created($"/api/v1/transacoes/{resultado.Data!.Id}", resultado.Data);
        })
        .WithName("Criar Transação")
        .WithDescription("Cria uma nova transação (compra, venda, dividendo, etc.)")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PUT /api/v1/transacoes/{id} - Atualizar transação
        group.MapPut("/{id:guid}", async (Guid id, TransacaoRequest request, HttpContext context, ITransacaoService service) =>
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
        .WithName("Atualizar Transação")
        .WithDescription("Atualiza uma transação existente")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // DELETE /api/v1/transacoes/{id} - Excluir transação
        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, ITransacaoService service) =>
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
        .WithName("Excluir Transação")
        .WithDescription("Exclui uma transação")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // Endpoints específicos de carteira
        var carteiraGroup = routes.MapGroup("/api/v1/carteiras/{carteiraId:long}/transacoes")
            .WithTags("Transações")
            .RequireAuthorization();

        // GET /api/v1/carteiras/{carteiraId}/transacoes - Listar transações da carteira
        carteiraGroup.MapGet("", async (long carteiraId, [AsParameters] GridifyQuery query, HttpContext context, ITransacaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterPorCarteiraAsync(carteiraId, query, usuarioId);

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
        .WithName("Listar Transações da Carteira")
        .WithDescription("Lista todas as transações de uma carteira com suporte a paginação e filtros (Gridify)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/carteiras/{carteiraId}/transacoes/periodo - Filtrar por período
        carteiraGroup.MapGet("/periodo", async (
            long carteiraId,
            DateTime inicio,
            DateTime fim,
            HttpContext context,
            ITransacaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ObterPorPeriodoAsync(carteiraId, inicio, fim, usuarioId);

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
        .WithName("Filtrar Transações por Período")
        .WithDescription("Filtra transações de uma carteira por período (query params: inicio e fim)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
