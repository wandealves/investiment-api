namespace Investment.Application.DTOs.Posicao;

public class PosicaoConsolidadaResponse
{
    public long CarteiraId { get; set; }
    public string CarteiraNome { get; set; } = default!;
    public DateTimeOffset DataCalculo { get; set; }

    public List<PosicaoAtivoResponse> Posicoes { get; set; } = new();

    public decimal ValorTotalInvestido { get; set; }
    public decimal? ValorTotalAtual { get; set; }
    public decimal? LucroTotal { get; set; }
    public decimal? RentabilidadeTotal { get; set; }

    public List<DistribuicaoTipoResponse> DistribuicaoPorTipo { get; set; } = new();
}
