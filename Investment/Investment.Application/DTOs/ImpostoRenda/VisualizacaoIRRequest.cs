namespace Investment.Application.DTOs.ImpostoRenda;

public class VisualizacaoIRRequest
{
    public int Ano { get; set; }
    public int? Mes { get; set; } // 1-12, null = todos os meses
    public long CarteiraId { get; set; }
}
