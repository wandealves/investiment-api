namespace Investment.Application.DTOs.ImpostoRenda;

public class CalculoIRResponse
{
    public Guid Id { get; set; }
    public int? Ano { get; set; }
    public DateTimeOffset DataCalculo { get; set; }

    public decimal ValorTotalInvestido { get; set; }
    public decimal? ValorTotalAtual { get; set; }
    public decimal TotalTaxasRateadas { get; set; }
    public decimal TotalGanhoCapital { get; set; }
    public decimal TotalIRDevido { get; set; }

    public List<ItemCalculoIRResponse> Itens { get; set; } = new();
}
