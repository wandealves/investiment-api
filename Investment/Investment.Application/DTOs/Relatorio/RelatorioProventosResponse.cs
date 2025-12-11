namespace Investment.Application.DTOs.Relatorio;

public class RelatorioProventosResponse
{
    public long CarteiraId { get; set; }
    public string CarteiraNome { get; set; } = default!;
    public DateTimeOffset DataInicio { get; set; }
    public DateTimeOffset DataFim { get; set; }
    public List<ProventoAtivoResponse> ProventosPorAtivo { get; set; } = new();
    public decimal TotalDividendos { get; set; }
    public decimal TotalJCP { get; set; }
    public decimal TotalGeral { get; set; }
}
