namespace Investment.Application.DTOs.Cotacao;

public class CotacaoResponse
{
    public long Id { get; set; }
    public long AtivoId { get; set; }
    public string AtivoNome { get; set; } = default!;
    public string AtivoCodigo { get; set; } = default!;
    public decimal Preco { get; set; }
    public DateTimeOffset DataHora { get; set; }
    public string TipoCotacao { get; set; } = default!;
    public decimal? PrecoAbertura { get; set; }
    public decimal? PrecoMaximo { get; set; }
    public decimal? PrecoMinimo { get; set; }
    public long? Volume { get; set; }
    public string? Fonte { get; set; }
}
