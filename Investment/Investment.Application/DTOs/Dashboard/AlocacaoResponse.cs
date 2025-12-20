using Investment.Domain.Common;

namespace Investment.Application.DTOs.Dashboard;

public class AlocacaoResponse
{
    public TipoAtivo Tipo { get; set; }
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
}
