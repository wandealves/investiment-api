namespace Investment.Application.DTOs.Relatorio;

public class RelatorioRentabilidadeResponse
{
    public long CarteiraId { get; set; }
    public string CarteiraNome { get; set; } = default!;
    public DateTimeOffset DataInicio { get; set; }
    public DateTimeOffset DataFim { get; set; }

    // Métricas financeiras
    public decimal? IRR { get; set; } // Internal Rate of Return (taxa anualizada %)
    public decimal? TWR { get; set; } // Time-Weighted Return (taxa do período %)
    public decimal RetornoSimples { get; set; } // Percentual

    // Valores
    public decimal ValorInicial { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal Aportes { get; set; }
    public decimal Resgates { get; set; }
    public decimal Proventos { get; set; }

    // Evolução mensal
    public List<RendimentoMensalResponse> RendimentoPorMes { get; set; } = new();
}
