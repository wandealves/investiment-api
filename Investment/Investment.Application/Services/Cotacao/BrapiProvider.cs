using Investment.Application.DTOs.Cotacao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Investment.Application.Services.Cotacao;

public class BrapiProvider : ICotacaoProviderStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrapiProvider> _logger;
    private readonly string? _apiKey;
    private const int MAX_CODIGOS_POR_REQUISICAO = 10;
    private static readonly SemaphoreSlim _semaphore = new(150, 150); // 150 req/min

    public string NomeProvedor => "BrapiDev";

    public BrapiProvider(HttpClient httpClient, ILogger<BrapiProvider> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["BrapiSettings:ApiKey"];

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Brapi API Key não configurada. Configure em appsettings.json > BrapiSettings:ApiKey");
        }
    }

    private string BuildUrl(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return endpoint;
        }

        var separator = endpoint.Contains('?') ? "&" : "?";
        return $"{endpoint}{separator}token={_apiKey}";
    }

    public async Task<CotacaoDto?> ObterCotacaoAsync(string codigo, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            // Endpoint: https://brapi.dev/api/quote/{ticker}?Authorization=bearer TOKEN
            var url = BuildUrl($"quote/{codigo}");
            var response = await _httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Brapi retornou {StatusCode} para {Codigo}",
                    response.StatusCode, codigo);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<BrapiQuoteResponse>(cancellationToken: ct);

            if (json?.Results == null || json.Results.Count == 0)
                return null;

            var result = json.Results[0];

            return new CotacaoDto
            {
                Codigo = result.Symbol,
                Preco = result.RegularMarketPrice,
                PrecoAbertura = result.RegularMarketOpen,
                PrecoMaximo = result.RegularMarketDayHigh,
                PrecoMinimo = result.RegularMarketDayLow,
                Volume = result.RegularMarketVolume,
                DataHora = DateTimeOffset.Parse(result.RegularMarketTime)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cotação de {Codigo} via Brapi", codigo);
            return null;
        }
        finally
        {
            // Liberar após 400ms (150 req/min = 1 req a cada 400ms)
            _ = Task.Run(async () =>
            {
                await Task.Delay(400, ct);
                _semaphore.Release();
            }, ct);
        }
    }

    public async Task<List<CotacaoDto>> ObterCotacoesEmLoteAsync(
        List<string> codigos,
        CancellationToken ct = default)
    {
        var cotacoes = new List<CotacaoDto>();

        // Dividir em batches de 10 (limite da API)
        //var batches = codigos.Chunk(MAX_CODIGOS_POR_REQUISICAO);

        foreach (var codido in codigos)
        {
           // var codigosJoin = string.Join(",", batch);

            await _semaphore.WaitAsync(ct);
            try
            {
                // Endpoint: https://brapi.dev/api/quote/PETR4?token=pjavsMmaaQ7RkRmU5y7Txz
                var url = BuildUrl($"quote/{codido}");
                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Brapi retornou {StatusCode} para lote: {Codigos}",
                        response.StatusCode, codido);
                    continue;
                }

                var json = await response.Content.ReadFromJsonAsync<BrapiQuoteResponse>(cancellationToken: ct);

                if (json?.Results != null)
                {
                    cotacoes.AddRange(json.Results.Select(r => new CotacaoDto
                    {
                        Codigo = r.Symbol,
                        Preco = r.RegularMarketPrice,
                        PrecoAbertura = r.RegularMarketOpen,
                        PrecoMaximo = r.RegularMarketDayHigh,
                        PrecoMinimo = r.RegularMarketDayLow,
                        Volume = r.RegularMarketVolume,
                        DataHora = DateTimeOffset.Parse(r.RegularMarketTime)
                    }));
                }

                // Rate limiting: aguardar entre batches
                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lote de cotações: {Codigos}", codido);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return cotacoes;
    }
}

// DTOs de resposta da Brapi
public class BrapiQuoteResponse
{
    public List<BrapiQuoteResult> Results { get; set; } = new();
}

public class BrapiQuoteResult
{
    public string Symbol { get; set; } = default!;
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketOpen { get; set; }
    public decimal RegularMarketDayHigh { get; set; }
    public decimal RegularMarketDayLow { get; set; }
    public long RegularMarketVolume { get; set; }
    public string RegularMarketTime { get; set; } = default!; // ISO 8601: "2025-12-22T18:54:10.000Z"
}
