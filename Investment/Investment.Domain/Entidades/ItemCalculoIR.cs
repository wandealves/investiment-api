namespace Investment.Domain.Entidades;

public class ItemCalculoIR
{
    public Guid Id { get; set; }
    public Guid CalculoIRId { get; set; }
    public long AtivoId { get; set; }

    public decimal Quantidade { get; set; }
    public decimal Total { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal? PrecoAtual { get; set; }
    public decimal? Rendimento { get; set; }
    public DateTimeOffset? Data { get; set; }

    // Rateio de taxas (conforme @regras.md)
    public decimal TaxasRateadas { get; set; }

    // Navegação
    public CalculoIR CalculoIR { get; set; } = default!;
    public Ativo Ativo { get; set; } = default!;

    public decimal TotalTaxa(IList<Transacao> transacoes)
    {
        return transacoes.FirstOrDefault(t => t.DataTransacao == Data)?.Taxa ?? 0;
    }

    public decimal Porcetagem(List<ItemCalculoIR> itensCalculo)
    {
        var totalInvestido = itensCalculo.FindAll(t => t.Data == Data).Sum(i => i.Total);
        return Total / totalInvestido;
    }
}