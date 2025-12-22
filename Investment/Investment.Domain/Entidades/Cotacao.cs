namespace Investment.Domain.Entidades;

public class Cotacao
{
    public long Id { get; set; }
    public long AtivoId { get; set; }

    // Preço principal
    public decimal Preco { get; set; }
    public DateTimeOffset DataHora { get; set; }
    public string TipoCotacao { get; set; } = "Fechamento"; // Abertura, Fechamento, Intraday

    // OHLCV (Open, High, Low, Close, Volume)
    public decimal? PrecoAbertura { get; set; }
    public decimal? PrecoMaximo { get; set; }
    public decimal? PrecoMinimo { get; set; }
    public long? Volume { get; set; }

    // Metadados
    public string? Fonte { get; set; }  // "BrapiDev", "YahooFinance", "AlphaVantage"

    // Relacionamento
    public Ativo Ativo { get; set; } = null!;
}
