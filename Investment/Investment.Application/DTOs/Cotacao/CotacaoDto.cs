namespace Investment.Application.DTOs.Cotacao;

public class CotacaoDto
{
    public string Codigo { get; set; } = default!;
    public decimal Preco { get; set; }
    public decimal? PrecoAbertura { get; set; }
    public decimal? PrecoMaximo { get; set; }
    public decimal? PrecoMinimo { get; set; }
    public long? Volume { get; set; }
    public DateTimeOffset DataHora { get; set; }
}
