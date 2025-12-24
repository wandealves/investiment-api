using Investment.Domain.Common;

namespace Investment.Domain.Entidades;

public class Provento
{
    public long Id { get; set; }
    public long AtivoId { get; set; }
    public TipoProvento TipoProvento { get; set; }
    public decimal ValorPorCota { get; set; }
    public DateTimeOffset DataCom { get; set; } // Data COM - precisa ter ação nesta data
    public DateTimeOffset? DataEx { get; set; } // Data EX - ação negocia sem direito ao provento
    public DateTimeOffset DataPagamento { get; set; } // Data efetiva do pagamento
    public StatusProvento Status { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    // Relacionamentos
    public Ativo Ativo { get; set; } = default!;
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
