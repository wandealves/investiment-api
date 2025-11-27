using Gridify;
using Investment.Application.DTOs;
using Investment.Domain.Common;

namespace Investment.Application.Services;

public interface IAtivoService
{
    Task<Result<AtivoResponse>> ObterPorIdAsync(long id);
    Task<Result<AtivoResponse>> ObterPorCodigoAsync(string codigo);
    Task<Result<List<AtivoResponse>>> ObterTodosAsync();
    Task<Result<List<AtivoResponse>>> BuscarAsync(string termo);
    Task<Result<Paging<AtivoResponse>>> ObterAsync(GridifyQuery query);
    Task<Result<AtivoResponse>> CriarAsync(AtivoRequest request);
    Task<Result<AtivoResponse>> AtualizarAsync(long id, AtivoRequest request);
    Task<Result> ExcluirAsync(long id);
}