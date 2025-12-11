namespace Investment.Application.DTOs.Relatorio;

public class ProventoAtivoResponse
{
    public long AtivoId { get; set; }
    public string Nome { get; set; } = default!;
    public string Codigo { get; set; } = default!;
    public decimal TotalRecebido { get; set; }
    public decimal Quantidade { get; set; }
    public DateTimeOffset? DataUltimoPagamento { get; set; }
}
