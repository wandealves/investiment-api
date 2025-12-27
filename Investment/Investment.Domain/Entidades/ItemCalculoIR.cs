namespace Investment.Domain.Entidades;

public class ItemCalculoIR
{
    public Guid Id { get; set; }
    public Guid CalculoIRId { get; set; }
    public long AtivoId { get; set; }

    public decimal Quantidade { get; set; }
    public decimal TotalInvestido { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal? PrecoAtual { get; set; }
    public decimal? Rendimento { get; set; }

    // Rateio de taxas (conforme @regras.md)
    public decimal TaxasRateadas { get; set; }

    // IR
    public decimal GanhoCapital { get; set; }
    public decimal IRDevido { get; set; }
    public decimal AliquotaIR { get; set; }  // 15.00, 20.00

    // Navegação
    public CalculoIR CalculoIR { get; set; } = default!;
    public Ativo Ativo { get; set; } = default!;
}
