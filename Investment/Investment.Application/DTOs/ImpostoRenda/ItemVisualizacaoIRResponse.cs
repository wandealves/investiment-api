using Investment.Domain.Common;

namespace Investment.Application.DTOs.ImpostoRenda;

public class ItemVisualizacaoIRResponse
{
    public string AtivoCodigo { get; set; } = string.Empty;
    public string AtivoNome { get; set; } = string.Empty;
    public TipoAtivo AtivoTipo { get; set; }
    public decimal QuantidadeTotal { get; set; }
    public decimal Total { get; set; }
    public int? ItensCount { get; set; } // Quantidade de registros agrupados (útil para debug)
}
