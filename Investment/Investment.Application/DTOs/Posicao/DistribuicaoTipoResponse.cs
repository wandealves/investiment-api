using Investment.Domain.Common;

namespace Investment.Application.DTOs.Posicao;

public class DistribuicaoTipoResponse
{
    public TipoAtivo Tipo { get; set; }
    public decimal ValorInvestido { get; set; }
    public decimal Percentual { get; set; }
    public int QuantidadeAtivos { get; set; }
}
