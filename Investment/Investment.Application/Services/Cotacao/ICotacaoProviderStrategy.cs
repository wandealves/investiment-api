using Investment.Application.DTOs.Cotacao;

namespace Investment.Application.Services.Cotacao;

public interface ICotacaoProviderStrategy
{
    Task<CotacaoDto?> ObterCotacaoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<List<CotacaoDto>> ObterCotacoesEmLoteAsync(List<string> codigos, CancellationToken cancellationToken = default);
    string NomeProvedor { get; }
}
