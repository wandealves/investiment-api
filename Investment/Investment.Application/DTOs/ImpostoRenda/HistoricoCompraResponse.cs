namespace Investment.Application.DTOs.ImpostoRenda;

public class HistoricoCompraResponse
{
    public Guid TransacaoId { get; set; }
    public DateTimeOffset DataCompra { get; set; }
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal Taxa { get; set; }
    public decimal TaxaRateada { get; set; }  // Taxa após rateio mensal
    public decimal TotalComTaxas { get; set; }
}
