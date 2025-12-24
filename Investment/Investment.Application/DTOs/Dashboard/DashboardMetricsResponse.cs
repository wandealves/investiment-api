namespace Investment.Application.DTOs.Dashboard;

public class DashboardMetricsResponse
{
    public decimal PatrimonioTotal { get; set; }
    public decimal RentabilidadeTotal { get; set; }
    public int QuantidadeCarteiras { get; set; }
    public int QuantidadeAtivos { get; set; }
    public MelhorPiorAtivoResponse? MelhorAtivo { get; set; }
    public MelhorPiorAtivoResponse? PiorAtivo { get; set; }
    public decimal ProventosRecebidos30Dias { get; set; }
    public decimal ProventosRecebidosAno { get; set; }
    public decimal ValorInvestidoTotal { get; set; }
}

public class MelhorPiorAtivoResponse
{
    public string Codigo { get; set; } = string.Empty;
    public decimal Rentabilidade { get; set; }
}

public class ProventoRecenteResponse
{
    public long Id { get; set; }
    public string AtivoCodigo { get; set; } = string.Empty;
    public string AtivoNome { get; set; } = string.Empty;
    public string TipoProvento { get; set; } = string.Empty;
    public decimal ValorPorCota { get; set; }
    public DateTimeOffset DataPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UltimaTransacaoResponse
{
    public Guid Id { get; set; }
    public string AtivoCodigo { get; set; } = string.Empty;
    public string CarteiraNome { get; set; } = string.Empty;
    public string TipoTransacao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTimeOffset DataTransacao { get; set; }
}

public class DistribuicaoCarteiraResponse
{
    public long CarteiraId { get; set; }
    public string CarteiraNome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
    public int QuantidadeAtivos { get; set; }
}
