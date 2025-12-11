namespace Investment.Domain.ValueObjects;

public class CustosNota
{
    public decimal TaxaLiquidacao { get; set; }
    public decimal Emolumentos { get; set; }
    public decimal ISS { get; set; }
    public decimal Corretagem { get; set; }
    public decimal OutrosCustos { get; set; }
    public decimal Total { get; set; }
}
