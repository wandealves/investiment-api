using Hangfire;
using Investment.Application.Services.Cotacao;

namespace Investment.Api.Endpoints;

public static class CotacaoEndpoint
{
    public static void RegistrarCotacaoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/cotacoes")
            .WithName("Cotações")
            .WithTags("Cotações")
            .RequireAuthorization();

        // GET /api/v1/cotacoes/{ativoId}/historico?inicio=&fim=
        group.MapGet("/{ativoId:long}/historico", async (
            long ativoId,
            DateTimeOffset? inicio,
            DateTimeOffset? fim,
            ICotacaoService service) =>
        {
            var inicioData = inicio ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var fimData = fim ?? DateTimeOffset.UtcNow;

            var resultado = await service.ObterHistoricoAsync(ativoId, inicioData, fimData);

            if (!resultado.IsSuccess)
            {
                return Results.NotFound(new
                {
                    errors = resultado.Errors,
                    validationErrors = resultado.ValidationErrors
                });
            }

            return Results.Ok(new
            {
                ativoId,
                periodo = new { inicio = inicioData, fim = fimData },
                total = resultado.Data!.Count,
                cotacoes = resultado.Data
            });
        })
        .WithName("Obter Histórico de Cotações")
        .WithDescription("Retorna o histórico de cotações de um ativo em um período")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/cotacoes/{ativoId}/atualizar (manual)
        group.MapPost("/{ativoId:long}/atualizar", async (
            long ativoId,
            ICotacaoService service) =>
        {
            try
            {
                await service.AtualizarCotacaoAtivoAsync(ativoId);
                return Results.Ok(new { mensagem = "Cotação atualizada com sucesso" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { mensagem = ex.Message });
            }
        })
        .WithName("Atualizar Cotação Manualmente")
        .WithDescription("Força a atualização da cotação de um ativo específico")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // POST /api/v1/cotacoes/atualizar-todas (manual)
        group.MapPost("/atualizar-todas", (ICotacaoService service) =>
        {
            BackgroundJob.Enqueue<ICotacaoService>(s => s.AtualizarTodasCotacoesAsync());
            return Results.Ok(new { mensagem = "Atualização de todas as cotações agendada" });
        })
        .WithName("Atualizar Todas as Cotações")
        .WithDescription("Agenda a atualização de todas as cotações em background")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status401Unauthorized);

        // GET /api/v1/cotacoes/{ativoId}/preco-atual
        group.MapGet("/{ativoId:long}/preco-atual", async (
            long ativoId,
            ICotacaoService service) =>
        {
            var preco = await service.ObterPrecoAtualAsync(ativoId);

            if (preco == null)
            {
                return Results.NotFound(new { mensagem = "Preço não disponível para este ativo" });
            }

            return Results.Ok(new { ativoId, precoAtual = preco });
        })
        .WithName("Obter Preço Atual")
        .WithDescription("Retorna o preço atual (em cache) de um ativo")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}
