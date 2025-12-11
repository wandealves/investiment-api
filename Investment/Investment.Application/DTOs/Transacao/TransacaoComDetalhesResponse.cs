using Investment.Application.DTOs.Carteira;

namespace Investment.Application.DTOs.Transacao;

public class TransacaoComDetalhesResponse : TransacaoResponse
{
    public CarteiraResponse Carteira { get; set; } = default!;
    public AtivoResponse Ativo { get; set; } = default!;
}
