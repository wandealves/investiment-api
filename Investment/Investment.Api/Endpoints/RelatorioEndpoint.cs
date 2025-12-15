using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class RelatorioEndpoint
{
    public static void RegistrarRelatorioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/relatorios")
            .WithTags("Relatórios")
            .RequireAuthorization();

        // GET /api/v1/relatorios/rentabilidade/{carteiraId}?inicio=&fim=
        group.MapGet("/rentabilidade/{carteiraId:long}", async (
            long carteiraId,
            DateTime inicio,
            DateTime fim,
            HttpContext context,
            IRelatorioService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.GerarRelatorioRentabilidadeAsync(carteiraId, inicio, fim, usuarioId);

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
        .WithName("Gerar Relatório de Rentabilidade")
        .WithDescription("Gera relatório de rentabilidade com IRR, TWR e retorno simples para um período específico")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/relatorios/proventos/{carteiraId}?inicio=&fim=
        group.MapGet("/proventos/{carteiraId:long}", async (
            long carteiraId,
            DateTime inicio,
            DateTime fim,
            HttpContext context,
            IRelatorioService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.GerarRelatorioProventosAsync(carteiraId, inicio, fim, usuarioId);

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
        .WithName("Gerar Relatório de Proventos")
        .WithDescription("Gera relatório de proventos (dividendos e JCP) recebidos no período")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/relatorios/{carteiraId}/pdf?inicio=&fim=
        group.MapGet("/{carteiraId:long}/pdf", async (
            long carteiraId,
            DateTime inicio,
            DateTime fim,
            HttpContext context,
            IRelatorioService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ExportarRelatorioPdfAsync(carteiraId, inicio, fim, usuarioId);

            if (!resultado.IsSuccess || resultado.Data == null)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors
                });
            }

            var fileName = $"relatorio_{carteiraId}_{inicio:yyyyMMdd}_{fim:yyyyMMdd}.pdf";
            return Results.File(resultado.Data, "application/pdf", fileName);
        })
        .WithName("Exportar Relatório PDF")
        .WithDescription("Exporta relatório completo em formato PDF")
        .Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/relatorios/{carteiraId}/excel?inicio=&fim=
        group.MapGet("/{carteiraId:long}/excel", async (
            long carteiraId,
            DateTime inicio,
            DateTime fim,
            HttpContext context,
            IRelatorioService service) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await service.ExportarRelatorioExcelAsync(carteiraId, inicio, fim, usuarioId);

            if (!resultado.IsSuccess || resultado.Data == null)
            {
                return Results.BadRequest(new
                {
                    errors = resultado.Errors
                });
            }

            var fileName = $"relatorio_{carteiraId}_{inicio:yyyyMMdd}_{fim:yyyyMMdd}.xlsx";
            return Results.File(
                resultado.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
        .WithName("Exportar Relatório Excel")
        .WithDescription("Exporta relatório completo em formato Excel (XLSX) com múltiplas planilhas")
        .Produces<byte[]>(StatusCodes.Status200OK, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
