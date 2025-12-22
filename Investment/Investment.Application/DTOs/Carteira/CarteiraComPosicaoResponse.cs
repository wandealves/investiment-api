namespace Investment.Application.DTOs.Carteira;

public class CarteiraComPosicaoResponse
{
    public long Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }
    public DateTimeOffset CriadaEm { get; set; }
    public int TotalAtivos { get; set; }
    public int TotalTransacoes { get; set; }

    // Dados de posição
    public decimal ValorTotal { get; set; }
    public decimal? LucroTotal { get; set; }
    public decimal? RentabilidadeTotal { get; set; }
}
