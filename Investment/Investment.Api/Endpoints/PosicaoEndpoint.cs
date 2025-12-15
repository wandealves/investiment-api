using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class PosicaoEndpoint
{
    public static void RegistrarPosicaoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/carteiras")
            .WithTags("Posição")
            .RequireAuthorization();

        // GET /api/v1/carteiras/{carteiraId}/posicao - Calcular posição consolidada
        group.MapGet("/{carteiraId:long}/posicao", async (
            long carteiraId,
            HttpContext context,
            IPosicaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.CalcularPosicaoAsync(carteiraId, usuarioId);

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
        .WithName("Calcular Posição Consolidada")
        .WithDescription("Calcula a posição consolidada da carteira com preço médio (WAC), quantidade atual e rentabilidade")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/carteiras/{carteiraId}/posicao/{ativoId} - Calcular posição de um ativo
        group.MapGet("/{carteiraId:long}/posicao/{ativoId:long}", async (
            long carteiraId,
            long ativoId,
            HttpContext context,
            IPosicaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.CalcularPosicaoAtivoAsync(carteiraId, ativoId, usuarioId);

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
        .WithName("Calcular Posição de Ativo")
        .WithDescription("Calcula a posição de um ativo específico na carteira")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/posicoes - Calcular todas as posições do usuário
        var posicoesGroup = routes.MapGroup("/api/v1/posicoes")
            .WithTags("Posição")
            .RequireAuthorization();

        posicoesGroup.MapGet("", async (HttpContext context, IPosicaoService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.CalcularTodasPosicoesAsync(usuarioId);

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
        .WithName("Calcular Todas as Posições")
        .WithDescription("Calcula a posição consolidada de todas as carteiras do usuário")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
