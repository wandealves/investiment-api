namespace Investment.Application.DTOs.ImpostoRenda;

public class CalculoIRResponse
{
    public Guid Id { get; set; }
    public int? Ano { get; set; }
    public DateTimeOffset Data { get; set; }

    public decimal Total { get; set; }
    public decimal? Valor { get; set; }
    public decimal TotalTaxas { get; set; }

    public List<ItemCalculoIRResponse> Itens { get; set; } = new();
}
