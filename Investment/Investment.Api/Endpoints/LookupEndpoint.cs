using Gridify;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;
public static class LookupEndpoint
{
    public static void RegistrarLookupEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/lookups")
            .WithName("Lookup")
            .WithTags("Lookup")
            .RequireAuthorization();

        group.MapGet("", async ([AsParameters] GridifyQuery query, ILookupService service) =>
        {
            var resultado = await service.ObterAtivosAsync(query);

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
        .WithName("Listar Lookup Ativos")
        .WithDescription("Lista todos os lookup ativos com suporte a paginação e filtros (Gridify)")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

    }
}

