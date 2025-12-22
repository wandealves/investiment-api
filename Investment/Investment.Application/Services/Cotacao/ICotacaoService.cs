using Investment.Application.DTOs.Cotacao;
using Investment.Domain.Common;

namespace Investment.Application.Services.Cotacao;

public interface ICotacaoService
{
    Task AtualizarTodasCotacoesAsync();
    Task AtualizarCotacaoAtivoAsync(long ativoId);
    Task<Result<List<CotacaoResponse>>> ObterHistoricoAsync(long ativoId, DateTimeOffset inicio, DateTimeOffset fim);
    Task<decimal?> ObterPrecoAtualAsync(long ativoId);
}
