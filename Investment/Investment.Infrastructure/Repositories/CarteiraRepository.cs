using Gridify;
using Gridify.EntityFramework;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class CarteiraRepository(InvestmentDbContext context) : ICarteiraRepository
{
    public async Task<Carteira> SalvarAsync(Carteira carteira)
    {
        if (carteira == null)
            throw new ArgumentNullException(nameof(carteira));
        var usuarioExiste = await context.Usuarios
            .AnyAsync(u => u.Id == carteira.UsuarioId)
            .ConfigureAwait(false);

        if (!usuarioExiste)
            throw new InvalidOperationException($"Usuário com ID '{carteira.UsuarioId}' não encontrado.");

        await context.Carteiras.AddAsync(carteira).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return carteira;
    }

    public async Task<Carteira> AtualizarAsync(Carteira carteira)
    {
        if (carteira == null)
            throw new ArgumentNullException(nameof(carteira));

        var carteiraExistente = await context.Carteiras
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == carteira.Id)
            .ConfigureAwait(false);

        if (carteiraExistente == null)
            throw new InvalidOperationException($"Carteira com ID '{carteira.Id}' não encontrada.");

        var usuarioExiste = await context.Usuarios
            .AnyAsync(u => u.Id == carteira.UsuarioId)
            .ConfigureAwait(false);

        if (!usuarioExiste)
            throw new InvalidOperationException($"Usuário com ID '{carteira.UsuarioId}' não encontrado.");

        context.Carteiras.Update(carteira);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return carteira;
    }

    public async Task<Carteira?> ObterPorIdAsync(long id)
    {
        return await context.Carteiras
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<Carteira>> ObterTodosAsync()
    {
        return await context.Carteiras
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Carteira>> ObterPorUsuarioIdAsync(Guid usuarioId)
    {
        return await context.Carteiras
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Carteira>> BuscarPorNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return await ObterTodosAsync().ConfigureAwait(false);

        var termoBusca = nome.ToLower().Trim();

        return await context.Carteiras
            .AsNoTracking()
            .Where(c => c.Nome.ToLower().Contains(termoBusca))
            .OrderBy(c => c.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Paging<Carteira>> ObterAsync(GridifyQuery query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryable = context.Carteiras.AsNoTracking();

        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<Paging<Carteira>> ObterPorUsuarioAsync(GridifyQuery query, Guid usuarioId)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryable = context.Carteiras
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId);

        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<List<Carteira>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await context.Carteiras
            .AsNoTracking()
            .Where(c => c.CriadaEm >= inicio && c.CriadaEm <= fim)
            .OrderBy(c => c.CriadaEm)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Carteira?> ObterComTransacoesAsync(long id)
    {
        return await context.Carteiras
            .AsNoTracking()
            .Include(c => c.Transacoes)
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Carteira?> ObterComAtivosAsync(long id)
    {
        return await context.Carteiras
            .AsNoTracking()
            .Include(c => c.CarteirasAtivos)
                .ThenInclude(ca => ca.Ativo)
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Carteira?> ObterCompletoAsync(long id)
    {
        return await context.Carteiras
            .AsNoTracking()
            .Include(c => c.Usuario)
            .Include(c => c.Transacoes)
            .Include(c => c.CarteirasAtivos)
                .ThenInclude(ca => ca.Ativo)
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var carteira = await context.Carteiras
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);

        if (carteira == null)
            return false;

        context.Carteiras.Remove(carteira);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }

    public async Task<bool> UsuarioPossuiCarteiraAsync(Guid usuarioId, long carteiraId)
    {
        return await context.Carteiras
            .AsNoTracking()
            .AnyAsync(c => c.Id == carteiraId && c.UsuarioId == usuarioId)
            .ConfigureAwait(false);
    }
}
