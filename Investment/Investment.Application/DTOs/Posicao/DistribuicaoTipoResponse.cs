namespace Investment.Application.DTOs.Posicao;

public class DistribuicaoTipoResponse
{
    public string Tipo { get; set; } = default!;
    public decimal ValorInvestido { get; set; }
    public decimal Percentual { get; set; }
    public int QuantidadeAtivos { get; set; }
}
