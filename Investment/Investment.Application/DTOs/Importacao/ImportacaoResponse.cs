using Investment.Application.DTOs.Transacao;

namespace Investment.Application.DTOs.Importacao;

public class ImportacaoResponse
{
    public bool Sucesso { get; set; }
    public int TransacoesCriadas { get; set; }
    public int TransacoesIgnoradas { get; set; }
    public List<string> Erros { get; set; } = new();
    public List<string> Avisos { get; set; } = new();
    public List<TransacaoResponse> PreviewTransacoes { get; set; } = new();
}
