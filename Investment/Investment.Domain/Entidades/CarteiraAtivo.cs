namespace Investment.Domain.Entidades;

public class CarteiraAtivo
{
    public long Id { get; set; }

    public long CarteiraId { get; set; }
    public long AtivoId { get; set; }

    // Navegação
    public Carteira Carteira { get; set; } = default!;
    public Ativo Ativo { get; set; } = default!;
}