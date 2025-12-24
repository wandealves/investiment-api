using Investment.Domain.Common;

namespace Investment.Application.DTOs.Provento;

public class ProventoResponse
{
    public long Id { get; set; }
    public long AtivoId { get; set; }
    public string AtivoNome { get; set; } = default!;
    public string AtivoCodigo { get; set; } = default!;
    public TipoProvento TipoProvento { get; set; }
    public string TipoProventoDescricao { get; set; } = default!;
    public decimal ValorPorCota { get; set; }
    public DateTimeOffset DataCom { get; set; }
    public DateTimeOffset? DataEx { get; set; }
    public DateTimeOffset DataPagamento { get; set; }
    public StatusProvento Status { get; set; }
    public string StatusDescricao { get; set; } = default!;
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
