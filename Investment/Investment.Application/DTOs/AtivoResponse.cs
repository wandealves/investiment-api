namespace Investment.Application.DTOs;

public class AtivoResponse
{
    public long Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Codigo { get; set; } = default!;
    public string Tipo { get; set; } = default!;
    public string? Descricao { get; set; }
}
