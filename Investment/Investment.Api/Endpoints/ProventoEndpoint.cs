using Gridify;
using Investment.Application.DTOs.Provento;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class ProventoEndpoint
{
    public static void RegistrarProventoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/proventos")
            .WithName("Proventos")
            .WithTags("Proventos")
            .RequireAuthorization();

        // GET /api/v1/proventos - Listar todos os proventos
        group.MapGet("", async ([AsParameters] GridifyQuery query, IProventoService service) =>
        {
            var resultado = await service.ObterAsync(query);

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
        .WithName("Listar Proventos")
        .WithDescription("Lista todos os proventos com suporte a paginação e filtros (Gridify)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/proventos/{id} - Obter provento por ID
        group.MapGet("/{id:long}", async (long id, IProventoService service) =>
        {
            var resultado = await service.ObterPorIdAsync(id);

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
        .WithName("Obter Provento por ID")
        .WithDescription("Obtém um provento específico")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/proventos/{id}/detalhes - Obter provento com transações
        group.MapGet("/{id:long}/detalhes", async (long id, IProventoService service) =>
        {
            var resultado = await service.ObterComTransacoesAsync(id);

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
        .WithName("Obter Provento com Transações")
        .WithDescription("Obtém um provento com todas as transações vinculadas")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/proventos/agendados - Listar proventos agendados
        group.MapGet("/agendados", async (IProventoService service) =>
        {
            var resultado = await service.ObterAgendadosAsync();

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
        .WithName("Listar Proventos Agendados")
        .WithDescription("Lista todos os proventos com status 'Agendado'")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/proventos/ativo/{ativoId} - Listar proventos por ativo
        group.MapGet("/ativo/{ativoId:long}", async (long ativoId, IProventoService service) =>
        {
            var resultado = await service.ObterPorAtivoAsync(ativoId);

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
        .WithName("Listar Proventos por Ativo")
        .WithDescription("Lista todos os proventos de um ativo específico")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/proventos/periodo - Filtrar por período
        group.MapGet("/periodo", async (
            DateTimeOffset inicio,
            DateTimeOffset fim,
            IProventoService service) =>
        {
            var resultado = await service.ObterPorPeriodoAsync(inicio, fim);

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
        .WithName("Filtrar Proventos por Período")
        .WithDescription("Filtra proventos por período de pagamento (query params: inicio e fim)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/proventos - Criar novo provento
        group.MapPost("", async (ProventoRequest request, IProventoService service) =>
        {
            var resultado = await service.CriarAsync(request);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Created($"/api/v1/proventos/{resultado.Data!.Id}", resultado.Data);
        })
        .WithName("Criar Provento")
        .WithDescription("Cria um novo provento (dividendo, JCP, rendimento FII, bonificação)")
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PUT /api/v1/proventos/{id} - Atualizar provento
        group.MapPut("/{id:long}", async (long id, ProventoRequest request, IProventoService service) =>
        {
            var resultado = await service.AtualizarAsync(id, request);

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
        .WithName("Atualizar Provento")
        .WithDescription("Atualiza um provento existente")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PATCH /api/v1/proventos/{id}/marcar-pago - Marcar como pago
        group.MapPatch("/{id:long}/marcar-pago", async (long id, IProventoService service) =>
        {
            var resultado = await service.MarcarComoPagoAsync(id);

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
        .WithName("Marcar Provento como Pago")
        .WithDescription("Altera o status do provento para 'Pago'")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // PATCH /api/v1/proventos/{id}/cancelar - Cancelar provento
        group.MapPatch("/{id:long}/cancelar", async (long id, IProventoService service) =>
        {
            var resultado = await service.CancelarAsync(id);

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
        .WithName("Cancelar Provento")
        .WithDescription("Altera o status do provento para 'Cancelado'")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // DELETE /api/v1/proventos/{id} - Excluir provento
        group.MapDelete("/{id:long}", async (long id, IProventoService service) =>
        {
            var resultado = await service.ExcluirAsync(id);

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
        .WithName("Excluir Provento")
        .WithDescription("Exclui um provento")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
