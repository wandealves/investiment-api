using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class CarteiraAtivoRepository(InvestmentDbContext context) : ICarteiraAtivoRepository
{
    public async Task<CarteiraAtivo> SalvarAsync(CarteiraAtivo carteiraAtivo)
    {
        if (carteiraAtivo == null)
            throw new ArgumentNullException(nameof(carteiraAtivo));
        var carteiraExiste = await context.Carteiras
            .AnyAsync(c => c.Id == carteiraAtivo.CarteiraId)
            .ConfigureAwait(false);

        if (!carteiraExiste)
            throw new InvalidOperationException($"Carteira com ID '{carteiraAtivo.CarteiraId}' não encontrada.");
        var ativoExiste = await context.Ativos
            .AnyAsync(a => a.Id == carteiraAtivo.AtivoId)
            .ConfigureAwait(false);

        if (!ativoExiste)
            throw new InvalidOperationException($"Ativo com ID '{carteiraAtivo.AtivoId}' não encontrado.");
        var relacionamentoExiste = await ExistePorCarteiraEAtivoAsync(carteiraAtivo.CarteiraId, carteiraAtivo.AtivoId)
            .ConfigureAwait(false);

        if (relacionamentoExiste)
            throw new InvalidOperationException($"O ativo já está associado a esta carteira.");

        await context.CarteirasAtivos.AddAsync(carteiraAtivo).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return carteiraAtivo;
    }

    public async Task<CarteiraAtivo?> ObterPorIdAsync(long id)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .FirstOrDefaultAsync(ca => ca.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<CarteiraAtivo>> ObterPorCarteiraIdAsync(long carteiraId)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .Where(ca => ca.CarteiraId == carteiraId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<CarteiraAtivo>> ObterPorAtivoIdAsync(long ativoId)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .Where(ca => ca.AtivoId == ativoId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<CarteiraAtivo?> ObterPorCarteiraEAtivoAsync(long carteiraId, long ativoId)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .FirstOrDefaultAsync(ca => ca.CarteiraId == carteiraId && ca.AtivoId == ativoId)
            .ConfigureAwait(false);
    }

    public async Task<List<CarteiraAtivo>> ObterComDetalhesAsync(long carteiraId)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .Include(ca => ca.Ativo)
            .Include(ca => ca.Carteira)
            .Where(ca => ca.CarteiraId == carteiraId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var carteiraAtivo = await context.CarteirasAtivos
            .FirstOrDefaultAsync(ca => ca.Id == id)
            .ConfigureAwait(false);

        if (carteiraAtivo == null)
            return false;

        context.CarteirasAtivos.Remove(carteiraAtivo);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }

    public async Task<bool> RemoverPorCarteiraEAtivoAsync(long carteiraId, long ativoId)
    {
        var carteiraAtivo = await context.CarteirasAtivos
            .FirstOrDefaultAsync(ca => ca.CarteiraId == carteiraId && ca.AtivoId == ativoId)
            .ConfigureAwait(false);

        if (carteiraAtivo == null)
            return false;

        context.CarteirasAtivos.Remove(carteiraAtivo);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }

    public async Task<bool> ExistePorCarteiraEAtivoAsync(long carteiraId, long ativoId)
    {
        return await context.CarteirasAtivos
            .AsNoTracking()
            .AnyAsync(ca => ca.CarteiraId == carteiraId && ca.AtivoId == ativoId)
            .ConfigureAwait(false);
    }
}
