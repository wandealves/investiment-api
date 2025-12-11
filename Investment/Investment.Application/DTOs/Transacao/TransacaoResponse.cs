namespace Investment.Application.DTOs.Transacao;

public class TransacaoResponse
{
    public Guid Id { get; set; }
    public long CarteiraId { get; set; }
    public long AtivoId { get; set; }
    public string AtivoNome { get; set; } = default!;
    public string AtivoCodigo { get; set; } = default!;
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal ValorTotal { get; set; }
    public string TipoTransacao { get; set; } = default!;
    public DateTimeOffset DataTransacao { get; set; }
}
