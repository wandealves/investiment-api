using Investment.Domain.Common;

namespace Investment.Application.DTOs.Posicao;

public class PosicaoAtivoResponse
{
    public long AtivoId { get; set; }
    public string AtivoNome { get; set; } = default!;
    public string AtivoCodigo { get; set; } = default!;
    public TipoAtivo AtivoTipo { get; set; }

    public decimal QuantidadeAtual { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorInvestido { get; set; } // QuantidadeAtual * PrecoMedio

    // Opcional - para quando houver integração com API de cotações
    public decimal? PrecoAtual { get; set; }
    public decimal? ValorAtual { get; set; } // QuantidadeAtual * PrecoAtual
    public decimal? Lucro { get; set; } // ValorAtual - ValorInvestido
    public decimal? Rentabilidade { get; set; } // (Lucro / ValorInvestido) * 100

    public decimal DividendosRecebidos { get; set; }
    public DateTimeOffset? DataPrimeiraCompra { get; set; }
    public DateTimeOffset? DataUltimaTransacao { get; set; }
}
