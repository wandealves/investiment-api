namespace Investment.Application.DTOs.Dashboard;

public class AlocacaoResponse
{
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
}
