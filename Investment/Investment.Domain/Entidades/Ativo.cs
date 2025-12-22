using Investment.Domain.Common;

namespace Investment.Domain.Entidades;

public class Ativo
{
    public long Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Codigo { get; set; } = default!; // B3: PETR4, IVVB11, HASH11...
    public TipoAtivo Tipo { get; set; }
    public string? Descricao { get; set; }

    // Cotação em cache (última atualização)
    public decimal? PrecoAtual { get; set; }
    public DateTimeOffset? PrecoAtualizadoEm { get; set; }
    public string? FonteCotacao { get; set; }

    // Relacionamentos
    public ICollection<CarteiraAtivo> CarteirasAtivos { get; set; } = new List<CarteiraAtivo>();
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<Cotacao> Cotacoes { get; set; } = new List<Cotacao>();
}