using Gridify;
using Investment.Application.DTOs;
using Investment.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Investment.Api.Endpoints;

public static class AtivoEndpoint
{
    public static void RegistrarAtivoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ativos")
            .WithName("Ativos")
            .WithTags("Ativos");

        // GET /api/v1/ativos - Listar todos os ativos com paginação e filtros
        group.MapGet("", async ([AsParameters] GridifyQuery query, IAtivoService service) =>
        {
            var resultado = await service.ObterAsync(query);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Listar Ativos")
        .WithDescription("Lista todos os ativos com suporte a paginação e filtros (Gridify)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
        
        // GET /api/v1/ativos/buscar/{termo} - Buscar ativos por termo
        group.MapGet("/termo/{termo}", async (string termo, IAtivoService service) =>
        {
            var resultado = await service.BuscarAsync(termo);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Buscar Ativos")
        .WithDescription("Busca ativos por termo (nome, código ou tipo)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // GET /api/v1/ativos/{id} - Buscar ativo por ID
        group.MapGet("/{id:long}", async (long id, IAtivoService service) =>
        {
            var resultado = await service.ObterPorIdAsync(id);

            if (!resultado.IsSuccess)
            {
                return Results.NotFound(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Obter Ativo por ID")
        .WithDescription("Obtém um ativo específico pelo ID")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);

        // GET /api/v1/ativos/codigo/{codigo} - Buscar ativo por código
        group.MapGet("/codigo/{codigo}", async (string codigo, IAtivoService service) =>
        {
            var resultado = await service.ObterPorCodigoAsync(codigo);

            if (!resultado.IsSuccess)
            {
                return Results.NotFound(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Obter Ativo por Código")
        .WithDescription("Obtém um ativo específico pelo código (ex: PETR4, IVVB11)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);

        // POST /api/v1/ativos - Criar novo ativo
        group.MapPost("", async (AtivoRequest request, IAtivoService service) =>
        {
            var resultado = await service.CriarAsync(request);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Created($"/api/v1/ativos/{resultado.Data!.Id}", new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Criar Ativo")
        .WithDescription("Cria um novo ativo")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // PUT /api/v1/ativos/{id} - Atualizar ativo
        group.MapPut("/{id:long}", async (long id, AtivoRequest request, IAtivoService service) =>
        {
            var resultado = await service.AtualizarAsync(id, request);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Atualizar Ativo")
        .WithDescription("Atualiza um ativo existente")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status404NotFound);

        // DELETE /api/v1/ativos/{id} - Excluir ativo
        group.MapDelete("/{id:long}", async (long id, IAtivoService service) =>
        {
            var resultado = await service.ExcluirAsync(id);

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
        .WithName("Excluir Ativo")
        .WithDescription("Exclui um ativo")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}