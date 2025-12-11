using Investment.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Investment.Api.Endpoints;

public static class ImportacaoEndpoint
{
    private const long TamanhoMaximoArquivo = 5 * 1024 * 1024; // 5MB
    private const string TipoArquivoPdf = "application/pdf";

    public static void RegistrarImportacaoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/importacao")
            .WithTags("Importação")
            .RequireAuthorization();

        // POST /api/v1/importacao/preview - Preview sem salvar
        group.MapPost("/preview", async (
            [FromForm] long carteiraId,
            [FromForm] string corretoraTipo,
            IFormFile file,
            HttpContext context,
            IImportacaoService service) =>
        {
            // Validar arquivo
            var validacao = ValidarArquivo(file);
            if (validacao != null)
                return validacao;

            var usuarioId = context.GetUsuarioId();

            using var stream = file.OpenReadStream();
            var resultado = await service.PreviewImportacaoAsync(
                carteiraId, stream, corretoraTipo, usuarioId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    data = resultado.Data
                });
            }

            return Results.Ok(new
            {
                success = true,
                data = resultado.Data
            });
        })
        .WithName("Preview Importação")
        .WithDescription("Faz o preview da importação sem salvar as transações no banco de dados")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .DisableAntiforgery();

        // POST /api/v1/importacao/confirmar - Importar e salvar
        group.MapPost("/confirmar", async (
            [FromForm] long carteiraId,
            [FromForm] string corretoraTipo,
            IFormFile file,
            HttpContext context,
            IImportacaoService service) =>
        {
            // Validar arquivo
            var validacao = ValidarArquivo(file);
            if (validacao != null)
                return validacao;

            var usuarioId = context.GetUsuarioId();

            using var stream = file.OpenReadStream();
            var resultado = await service.ImportarNotaAsync(
                carteiraId, stream, corretoraTipo, usuarioId);

            if (!resultado.IsSuccess)
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errors = resultado.Errors,
                    data = resultado.Data
                });
            }

            return Results.Ok(new
            {
                success = true,
                message = $"{resultado.Data?.TransacoesCriadas} transação(ões) importada(s) com sucesso",
                data = resultado.Data
            });
        })
        .WithName("Confirmar Importação")
        .WithDescription("Importa as transações da nota de corretagem e salva no banco de dados")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .DisableAntiforgery();
    }

    private static IResult? ValidarArquivo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new
            {
                success = false,
                errors = new[] { "Arquivo não fornecido ou está vazio" }
            });
        }

        if (file.Length > TamanhoMaximoArquivo)
        {
            return Results.BadRequest(new
            {
                success = false,
                errors = new[] { $"Arquivo muito grande. Tamanho máximo: {TamanhoMaximoArquivo / 1024 / 1024}MB" }
            });
        }

        if (file.ContentType != TipoArquivoPdf)
        {
            return Results.BadRequest(new
            {
                success = false,
                errors = new[] { "Apenas arquivos PDF são permitidos" }
            });
        }

        // Verificar magic bytes PDF (%PDF)
        using var reader = new BinaryReader(file.OpenReadStream());
        var magicBytes = reader.ReadBytes(4);
        var isPdf = magicBytes.Length >= 4 &&
                    magicBytes[0] == 0x25 && // %
                    magicBytes[1] == 0x50 && // P
                    magicBytes[2] == 0x44 && // D
                    magicBytes[3] == 0x46;   // F

        if (!isPdf)
        {
            return Results.BadRequest(new
            {
                success = false,
                errors = new[] { "Arquivo não é um PDF válido" }
            });
        }

        return null;
    }
}
