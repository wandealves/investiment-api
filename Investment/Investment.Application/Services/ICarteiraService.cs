using Gridify;
using Investment.Application.DTOs.Carteira;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface ICarteiraService
{
    Task<Result<Paging<CarteiraResponse>>> ObterAsync(GridifyQuery query, Guid usuarioId);
    Task<Result<Paging<CarteiraComPosicaoResponse>>> ObterComPosicaoAsync(GridifyQuery query, Guid usuarioId);
    Task<Result<List<CarteiraResponse>>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<Result<CarteiraResponse>> ObterPorIdAsync(long id, Guid usuarioId);
    Task<Result<CarteiraComPosicaoResponse>> ObterComPosicaoPorIdAsync(long id, Guid usuarioId);
    Task<Result<CarteiraComDetalhesResponse>> ObterComDetalhesAsync(long id, Guid usuarioId);
    Task<Result<CarteiraResponse>> CriarAsync(CarteiraRequest request, Guid usuarioId);
    Task<Result<CarteiraResponse>> AtualizarAsync(long id, CarteiraRequest request, Guid usuarioId);
    Task<Result> ExcluirAsync(long id, Guid usuarioId);
}
