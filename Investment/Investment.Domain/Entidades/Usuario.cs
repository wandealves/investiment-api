namespace Investment.Domain.Entidades;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string SenhaHash { get; set; } = default!;
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    // Relacionamentos
    public ICollection<Carteira> Carteiras { get; set; } = new List<Carteira>();
}