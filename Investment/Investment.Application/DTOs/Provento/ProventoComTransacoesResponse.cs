using Investment.Application.DTOs.Transacao;

namespace Investment.Application.DTOs.Provento;

public class ProventoComTransacoesResponse : ProventoResponse
{
    public AtivoResponse Ativo { get; set; } = default!;
    public ICollection<TransacaoResponse> Transacoes { get; set; } = new List<TransacaoResponse>();
    public decimal ValorTotalPago { get; set; } // Soma de todas as transações vinculadas
}
