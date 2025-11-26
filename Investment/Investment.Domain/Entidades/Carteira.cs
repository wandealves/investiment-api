namespace Investment.Domain.Entidades;

public class Carteira
{
    public long Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = default!;
    public DateTimeOffset CriadaEm { get; set; } = DateTime.UtcNow;

    // Navegação
    public Usuario Usuario { get; set; } = default!;
    public ICollection<CarteiraAtivo> CarteirasAtivos { get; set; } = new List<CarteiraAtivo>();
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}