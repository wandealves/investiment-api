using Gridify;
using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface IAtivoRepository
{
    Task<Ativo?> ObterPorIdAsync(long id);
    Task<Ativo?> ObterPorCodigoAsync(string codigo);
    Task<List<Ativo>> ObterTodosAsync();
    Task<List<Ativo>> BuscarAsync(string termo);
    Task<Paging<Ativo>> ObterAsync(GridifyQuery query);
    Task<Ativo> SalvarAsync(Ativo ativo);
    Task<Ativo> AtualizarAsync(Ativo ativo);
    Task<bool> ExcluirAsync(long id);
    Task<bool> ExistePorCodigoAsync(string codigo, long? idExcluir = null);
}