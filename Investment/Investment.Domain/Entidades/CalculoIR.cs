namespace Investment.Domain.Entidades;

public class CalculoIR
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public int? Ano { get; set; }  // null = consolidado geral
    public DateTimeOffset DataCalculo { get; set; }

    // Totalizadores
    public decimal ValorTotalInvestido { get; set; }
    public decimal? ValorTotalAtual { get; set; }
    public decimal TotalTaxasRateadas { get; set; }
    public decimal TotalGanhoCapital { get; set; }
    public decimal TotalIRDevido { get; set; }

    // Navegação
    public Usuario Usuario { get; set; } = default!;
    public ICollection<ItemCalculoIR> Itens { get; set; } = new List<ItemCalculoIR>();
}
