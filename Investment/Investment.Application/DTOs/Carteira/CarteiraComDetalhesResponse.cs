using Investment.Application.DTOs.Transacao;

namespace Investment.Application.DTOs.Carteira;

public class CarteiraComDetalhesResponse : CarteiraResponse
{
    public List<AtivoResponse> Ativos { get; set; } = new();
    public List<TransacaoResponse> Transacoes { get; set; } = new();
}
