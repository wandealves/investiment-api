using Gridify;
using Gridify.EntityFramework;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class UsuarioRepository(InvestmentDbContext context) : IUsuarioRepository
{
    public async Task<Usuario> SalvarAsync(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));
        
        var emailJaExiste = await ExistePorEmailAsync(usuario.Email).ConfigureAwait(false);
        if (emailJaExiste)
            throw new InvalidOperationException($"Já existe um usuário com o email '{usuario.Email}'.");

        await context.Usuarios.AddAsync(usuario).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return usuario;
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));
        
        var usuarioExistente = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuario.Id)
            .ConfigureAwait(false);

        if (usuarioExistente == null)
            throw new InvalidOperationException($"Usuário com ID '{usuario.Id}' não encontrado.");
        
        var emailJaExiste = await ExistePorEmailAsync(usuario.Email, usuario.Id).ConfigureAwait(false);
        if (emailJaExiste)
            throw new InvalidOperationException($"Já existe outro usuário com o email '{usuario.Email}'.");

        context.Usuarios.Update(usuario);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return usuario;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email)
            .ConfigureAwait(false);
    }

    public async Task<List<Usuario>> ObterTodosAsync()
    {
        return await context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Usuario>> BuscarPorNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return await ObterTodosAsync().ConfigureAwait(false);

        var termoBusca = nome.ToLower().Trim();

        return await context.Usuarios
            .AsNoTracking()
            .Where(u => u.Nome.ToLower().Contains(termoBusca) ||
                       u.Email.ToLower().Contains(termoBusca))
            .OrderBy(u => u.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Paging<Usuario>> ObterAsync(GridifyQuery query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryable = context.Usuarios.AsNoTracking();

        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<List<Usuario>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await context.Usuarios
            .AsNoTracking()
            .Where(u => u.CriadoEm >= inicio && u.CriadoEm <= fim)
            .OrderBy(u => u.CriadoEm)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Usuario?> ObterComCarteirasAsync(Guid id)
    {
        return await context.Usuarios
            .AsNoTracking()
            .Include(u => u.Carteiras)
            .FirstOrDefaultAsync(u => u.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id)
            .ConfigureAwait(false);

        if (usuario == null)
            return false;

        context.Usuarios.Remove(usuario);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }

    public async Task<bool> ExistePorEmailAsync(string email, Guid? idExcluir = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = context.Usuarios.AsNoTracking();

        if (idExcluir.HasValue)
        {
            return await query
                .AnyAsync(u => u.Email == email && u.Id != idExcluir.Value)
                .ConfigureAwait(false);
        }

        return await query
            .AnyAsync(u => u.Email == email)
            .ConfigureAwait(false);
    }
}
