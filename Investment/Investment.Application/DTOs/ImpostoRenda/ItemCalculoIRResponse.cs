using Investment.Domain.Common;

namespace Investment.Application.DTOs.ImpostoRenda;

public class ItemCalculoIRResponse
{
    public long AtivoId { get; set; }
    public string AtivoNome { get; set; } = string.Empty;
    public string AtivoCodigo { get; set; } = string.Empty;
    public TipoAtivo AtivoTipo { get; set; }

    public decimal Quantidade { get; set; }
    public decimal TotalInvestido { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal? PrecoAtual { get; set; }
    public decimal? Rendimento { get; set; }

    public decimal TaxasRateadas { get; set; }
    public decimal GanhoCapital { get; set; }
    public decimal IRDevido { get; set; }
    public decimal AliquotaIR { get; set; }

    // Histórico de compras (para visualização)
    public List<HistoricoCompraResponse> HistoricoCompras { get; set; } = new();
}
