namespace Investment.Domain.ValueObjects;

public class NotaCorretagem
{
    public string NumeroNota { get; set; } = default!;
    public string Corretora { get; set; } = default!;
    public DateTimeOffset DataPregao { get; set; }
    public List<OperacaoNota> Operacoes { get; set; } = new();
    public CustosNota Custos { get; set; } = default!;
}
