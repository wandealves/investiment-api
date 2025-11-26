namespace Investment.Domain.Entidades;

public class Ativo
{
    public long Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Codigo { get; set; } = default!; // B3: PETR4, IVVB11, HASH11...
    public string Tipo { get; set; } = default!;   // Ação, ETF, FII, Cripto, RF...
    public string? Descricao { get; set; }

    // Relacionamentos
    public ICollection<CarteiraAtivo> CarteirasAtivos { get; set; } = new List<CarteiraAtivo>();
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}