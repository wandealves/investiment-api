namespace Investment.Domain.Entidades;

public class Transacao
{
    public Guid Id { get; set; }

    public long CarteiraId { get; set; }
    public long AtivoId { get; set; }

    public decimal Quantidade { get; set; }          // +compra / -venda
    public decimal Preco { get; set; }               // Preço unitário
    public string TipoTransacao { get; set; } = default!;
    // Compra, Venda, Dividendo, JCP, Split, Aporte, etc.

    public DateTimeOffset DataTransacao { get; set; }

    // Vínculo com provento (opcional - apenas para transações de dividendos/JCP)
    public long? ProventoId { get; set; }

    // Navegação
    public Carteira Carteira { get; set; } = default!;
    public Ativo Ativo { get; set; } = default!;
    public Provento? Provento { get; set; }
}