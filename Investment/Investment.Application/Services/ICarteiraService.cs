using Investment.Application.DTOs.Carteira;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface ICarteiraService
{
    Task<Result<List<CarteiraResponse>>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<Result<CarteiraResponse>> ObterPorIdAsync(long id, Guid usuarioId);
    Task<Result<CarteiraComDetalhesResponse>> ObterComDetalhesAsync(long id, Guid usuarioId);
    Task<Result<CarteiraResponse>> CriarAsync(CarteiraRequest request, Guid usuarioId);
    Task<Result<CarteiraResponse>> AtualizarAsync(long id, CarteiraRequest request, Guid usuarioId);
    Task<Result> ExcluirAsync(long id, Guid usuarioId);
}
