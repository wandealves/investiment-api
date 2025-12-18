using Gridify;
using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface ICarteiraRepository
{
    Task<Carteira?> ObterPorIdAsync(long id);
    Task<List<Carteira>> ObterTodosAsync();
    Task<List<Carteira>> ObterPorUsuarioIdAsync(Guid usuarioId);
    Task<List<Carteira>> BuscarPorNomeAsync(string nome);
    Task<Paging<Carteira>> ObterAsync(GridifyQuery query);
    Task<Paging<Carteira>> ObterPorUsuarioAsync(GridifyQuery query, Guid usuarioId);
    Task<List<Carteira>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<Carteira?> ObterComTransacoesAsync(long id);
    Task<Carteira?> ObterComAtivosAsync(long id);
    Task<Carteira?> ObterCompletoAsync(long id);
    Task<Carteira> SalvarAsync(Carteira carteira);
    Task<Carteira> AtualizarAsync(Carteira carteira);
    Task<bool> ExcluirAsync(long id);
    Task<bool> UsuarioPossuiCarteiraAsync(Guid usuarioId, long carteiraId);
}
