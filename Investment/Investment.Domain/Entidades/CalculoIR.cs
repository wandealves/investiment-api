namespace Investment.Domain.Entidades;

public class CalculoIR
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public int? Ano { get; set; }  // null = consolidado geral
    public DateTimeOffset Data { get; set; }

    // Totalizadores
    public decimal Total { get; set; }
    public decimal? Valor { get; set; }
    public decimal TotalTaxas { get; set; }
    // public decimal TotalGanhoCapital { get; set; }
    //public decimal TotalIRDevido { get; set; }

    // Navegação
    public Usuario Usuario { get; set; } = default!;
    public ICollection<ItemCalculoIR> Itens { get; set; } = new List<ItemCalculoIR>();
}
