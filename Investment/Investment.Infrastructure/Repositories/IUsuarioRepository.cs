using Gridify;
using Investment.Domain.Entidades;

namespace Investment.Infrastructure.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<List<Usuario>> ObterTodosAsync();
    Task<List<Usuario>> BuscarPorNomeAsync(string nome);
    Task<Paging<Usuario>> ObterAsync(GridifyQuery query);
    Task<List<Usuario>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<Usuario?> ObterComCarteirasAsync(Guid id);
    Task<Usuario> SalvarAsync(Usuario usuario);
    Task<Usuario> AtualizarAsync(Usuario usuario);
    Task<bool> ExcluirAsync(Guid id);
    Task<bool> ExistePorEmailAsync(string email, Guid? idExcluir = null);
}
