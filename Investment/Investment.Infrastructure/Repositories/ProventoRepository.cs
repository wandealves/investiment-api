using Gridify;
using Gridify.EntityFramework;
using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class ProventoRepository(InvestmentDbContext context) : IProventoRepository
{
    public async Task<Provento> SalvarAsync(Provento provento)
    {
        if (provento == null)
            throw new ArgumentNullException(nameof(provento));

        var ativoExiste = await context.Ativos
            .AnyAsync(a => a.Id == provento.AtivoId)
            .ConfigureAwait(false);

        if (!ativoExiste)
            throw new InvalidOperationException($"Ativo com ID '{provento.AtivoId}' não encontrado.");

        await context.Proventos.AddAsync(provento).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return provento;
    }

    public async Task<Provento> AtualizarAsync(Provento provento)
    {
        if (provento == null)
            throw new ArgumentNullException(nameof(provento));

        var proventoExistente = await context.Proventos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == provento.Id)
            .ConfigureAwait(false);

        if (proventoExistente == null)
            throw new InvalidOperationException($"Provento com ID '{provento.Id}' não encontrado.");

        var ativoExiste = await context.Ativos
            .AnyAsync(a => a.Id == provento.AtivoId)
            .ConfigureAwait(false);

        if (!ativoExiste)
            throw new InvalidOperationException($"Ativo com ID '{provento.AtivoId}' não encontrado.");

        context.Proventos.Update(provento);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return provento;
    }

    public async Task<Provento?> ObterPorIdAsync(long id)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterTodosAsync()
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Paging<Provento>> ObterAsync(GridifyQuery query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryable = context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo);

        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterPorAtivoIdAsync(long ativoId)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.AtivoId == ativoId)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterPorStatusAsync(StatusProvento status)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterPorPeriodoAsync(DateTimeOffset inicio, DateTimeOffset fim)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.DataPagamento >= inicio && p.DataPagamento <= fim)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterPorAtivoEPeriodoAsync(long ativoId, DateTimeOffset inicio, DateTimeOffset fim)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.AtivoId == ativoId &&
                       p.DataPagamento >= inicio &&
                       p.DataPagamento <= fim)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterAgendadosAsync()
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.Status == StatusProvento.Agendado)
            .OrderBy(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Provento>> ObterPagosPorAtivoAsync(long ativoId)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Where(p => p.AtivoId == ativoId && p.Status == StatusProvento.Pago)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Provento?> ObterComDetalhesAsync(long id)
    {
        return await context.Proventos
            .AsNoTracking()
            .Include(p => p.Ativo)
            .Include(p => p.Transacoes)
                .ThenInclude(t => t.Carteira)
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var provento = await context.Proventos
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);

        if (provento == null)
            return false;

        context.Proventos.Remove(provento);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }
}
