using Investment.Application.Services;

namespace Investment.Api.Endpoints;

public static class DashboardEndpoint
{
    public static void RegistrarDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        // GET /api/v1/dashboard/metrics - Obter métricas do dashboard
        group.MapGet("/metrics", async (
            HttpContext context,
            IDashboardService dashboardService) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await dashboardService.ObterMetricasAsync(usuarioId);

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
        .WithName("ObterMetricasDashboard")
        .WithOpenApi();

        // GET /api/v1/dashboard/alocacao - Obter alocação por tipo de ativo
        group.MapGet("/alocacao", async (
            HttpContext context,
            IDashboardService dashboardService) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await dashboardService.ObterAlocacaoAsync(usuarioId);

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
        .WithName("ObterAlocacaoDashboard")
        .WithOpenApi();

        // GET /api/v1/dashboard/evolucao - Obter evolução patrimonial
        group.MapGet("/evolucao", async (
            HttpContext context,
            IDashboardService dashboardService) =>
        {
            var usuarioId = context.GetUsuarioId();
            var resultado = await dashboardService.ObterEvolucaoPatrimonialAsync(usuarioId);

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
        .WithName("ObterEvolucaoDashboard")
        .WithOpenApi();
    }
}
