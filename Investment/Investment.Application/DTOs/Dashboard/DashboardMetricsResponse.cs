namespace Investment.Application.DTOs.Dashboard;

public class DashboardMetricsResponse
{
    public decimal PatrimonioTotal { get; set; }
    public decimal RentabilidadeTotal { get; set; }
    public int QuantidadeCarteiras { get; set; }
    public int QuantidadeAtivos { get; set; }
    public MelhorPiorAtivoResponse? MelhorAtivo { get; set; }
    public MelhorPiorAtivoResponse? PiorAtivo { get; set; }
}

public class MelhorPiorAtivoResponse
{
    public string Codigo { get; set; } = string.Empty;
    public decimal Rentabilidade { get; set; }
}
