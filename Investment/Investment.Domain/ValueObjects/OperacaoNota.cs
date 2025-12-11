namespace Investment.Domain.ValueObjects;

public class OperacaoNota
{
    public string Ticker { get; set; } = default!;
    public string TipoOperacao { get; set; } = default!; // "C" (Compra) ou "V" (Venda)
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TaxasProporcionais { get; set; }
}
