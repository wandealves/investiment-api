using Gridify;
using Gridify.EntityFramework;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class AtivoRepository(InvestmentDbContext context) : IAtivoRepository
{
    public async Task<Ativo> SalvarAsync(Ativo ativo)
    {
        if (ativo == null)
            throw new ArgumentNullException(nameof(ativo));
        var codigoJaExiste = await ExistePorCodigoAsync(ativo.Codigo).ConfigureAwait(false);
        if (codigoJaExiste)
            throw new InvalidOperationException($"Já existe um ativo com o código '{ativo.Codigo}'.");
        await context.Ativos.AddAsync(ativo).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return ativo;
    }

    public async Task<Ativo> AtualizarAsync(Ativo ativo)
    {
        if (ativo == null)
            throw new ArgumentNullException(nameof(ativo));
        var ativoExistente = await context.Ativos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == ativo.Id)
            .ConfigureAwait(false);
        if (ativoExistente == null)
            throw new InvalidOperationException($"Ativo com ID '{ativo.Id}' não encontrado.");
        var codigoJaExiste = await ExistePorCodigoAsync(ativo.Codigo, ativo.Id).ConfigureAwait(false);
        if (codigoJaExiste)
            throw new InvalidOperationException($"Já existe outro ativo com o código '{ativo.Codigo}'.");
        context.Ativos.Update(ativo);
        await context.SaveChangesAsync().ConfigureAwait(false);
        return ativo;
    }

    public async Task<Ativo?> ObterPorIdAsync(long id)
    {
        return await context.Ativos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Ativo?> ObterPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        return await context.Ativos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Codigo == codigo)
            .ConfigureAwait(false);
    }

    public async Task<List<Ativo>> ObterTodosAsync()
    {
        return await context.Ativos
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Ativo>> BuscarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return await ObterTodosAsync().ConfigureAwait(false);
        var termoBusca = termo.ToLower().Trim();
        return await context.Ativos
            .AsNoTracking()
            .Where(a => a.Nome.ToLower().Contains(termoBusca) ||
                        a.Codigo.ToLower().Contains(termoBusca))
            .OrderBy(a => a.Nome)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Paging<Ativo>> ObterAsync(GridifyQuery query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        var queryable = context.Ativos.AsNoTracking();
        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var ativo = await context.Ativos
            .FirstOrDefaultAsync(a => a.Id == id)
            .ConfigureAwait(false);
        if (ativo == null)
            return false;
        context.Ativos.Remove(ativo);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);
        return resultado > 0;
    }

    public async Task<bool> ExistePorCodigoAsync(string codigo, long? idExcluir = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;
        var query = context.Ativos.AsNoTracking();
        if (idExcluir.HasValue)
        {
            return await query
                .AnyAsync(a => a.Codigo == codigo && a.Id != idExcluir.Value)
                .ConfigureAwait(false);
        }

        return await query
            .AnyAsync(a => a.Codigo == codigo)
            .ConfigureAwait(false);
    }
}