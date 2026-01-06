using Investment.Application.DTOs.ImpostoRenda;
using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class ImpostoRendaEndpoint
{
    public static void RegistrarImpostoRendaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/impostoderenda")
            .WithName("Imposto de Renda")
            .WithTags("Imposto de Renda")
            .RequireAuthorization();

        // POST /api/v1/impostoderenda/calcular
        group.MapPost("/calcular", async (
                CalcularIRRequest request,
                HttpContext context,
                IImpostoRendaService service) =>
            {
                var usuarioId = context.GetUsuarioId();
                var resultado = await service.CalcularIRAsync(request.CarteiraId, request.Ano, usuarioId);

                if (!resultado.IsSuccess)
                    return Results.BadRequest(new { errors = resultado.Errors });

                return Results.Ok(resultado.Data);
            })
            .WithName("Calcular IR")
            .WithDescription("Calcula o Imposto de Renda com rateio de taxas e ganho de capital")
            .Produces<CalculoIRResponse>()
            .Produces<object>(400)
            .Produces<object>(401);

        // PUT /api/v1/impostoderenda/{id}/recalcular
        group.MapPut("/{id:guid}/recalcular", async (
                Guid id,
                HttpContext context,
                IImpostoRendaService service) =>
            {
                var usuarioId = context.GetUsuarioId();
                var resultado = await service.RecalcularIRAsync(id, usuarioId);

                if (!resultado.IsSuccess)
                    return Results.BadRequest(new { errors = resultado.Errors });

                return Results.Ok(resultado.Data);
            })
            .WithName("Recalcular IR")
            .WithDescription("Recalcula um cálculo de IR existente com dados atualizados")
            .Produces<CalculoIRResponse>()
            .Produces<object>(400)
            .Produces<object>(401);

        // GET /api/v1/impostoderenda/historico
        group.MapGet("/historico", async (
                HttpContext context,
                IImpostoRendaService service) =>
            {
                var usuarioId = context.GetUsuarioId();
                var resultado = await service.ObterHistoricoCalculosAsync(usuarioId);

                if (!resultado.IsSuccess)
                    return Results.BadRequest(new { errors = resultado.Errors });

                return Results.Ok(resultado.Data);
            })
            .WithName("Obter Histórico de Cálculos IR")
            .WithDescription("Retorna todos os cálculos de IR realizados pelo usuário")
            .Produces<List<CalculoIRResponse>>()
            .Produces<object>(400)
            .Produces<object>(401);

        // DELETE /api/v1/impostoderenda/{id}
        group.MapDelete("/{id:guid}", async (
                Guid id,
                HttpContext context,
                IImpostoRendaService service) =>
            {
                var usuarioId = context.GetUsuarioId();
                var resultado = await service.ExcluirCalculoAsync(id, usuarioId);

                if (!resultado.IsSuccess)
                    return Results.BadRequest(new { errors = resultado.Errors });

                return Results.Ok(new { success = true });
            })
            .WithName("Excluir Cálculo IR")
            .WithDescription("Exclui um cálculo de IR do histórico")
            .Produces<object>()
            .Produces<object>(400)
            .Produces<object>(401);

        // GET /api/v1/impostoderenda/visualizacao?ano=2024&mes=1&carteiraId=5
        group.MapGet("/visualizacao", async (
                int ano,
                int? mes,
                long carteiraId,
                HttpContext context,
                IImpostoRendaService service) =>
            {
                var usuarioId = context.GetUsuarioId();
                var resultado = await service.ObterVisualizacaoPorPeriodoAsync(ano, mes, carteiraId, usuarioId);

                if (!resultado.IsSuccess)
                    return Results.BadRequest(new { errors = resultado.Errors });

                return Results.Ok(resultado.Data);
            })
            .WithName("Obter Visualização de IR por Período")
            .WithDescription(
                "Retorna os ativos agrupados e somados por código, filtrados por ano, mês (opcional) e carteira")
            .Produces<List<ItemVisualizacaoIRResponse>>()
            .Produces<object>(400)
            .Produces<object>(401);
    }
}