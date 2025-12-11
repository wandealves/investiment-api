namespace Investment.Application.DTOs.Relatorio;

public class RendimentoMensalResponse
{
    public string Mes { get; set; } = default!; // Formato: "YYYY-MM"
    public decimal Rentabilidade { get; set; } // Percentual
    public decimal ValorFinal { get; set; }
}
