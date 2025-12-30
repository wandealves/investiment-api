using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class CalculoIRRepository : ICalculoIRRepository
{
    private readonly InvestmentDbContext _context;

    public CalculoIRRepository(InvestmentDbContext context)
    {
        _context = context;
    }

    public async Task<CalculoIR?> ObterPorIdAsync(Guid id)
    {
        return await _context.CalculosIR
            .AsNoTracking()
            .Include(c => c.Itens)
                .ThenInclude(i => i.Ativo)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<CalculoIR>> ObterPorUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.CalculosIR
            .AsNoTracking()
            .Include(c => c.Itens)
                .ThenInclude(i => i.Ativo)
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.Data)
            .ToListAsync();
    }

    public async Task<CalculoIR?> ObterUltimoPorUsuarioEAnoAsync(Guid usuarioId, int? ano)
    {
        return await _context.CalculosIR
            .AsNoTracking()
            .Include(c => c.Itens)
                .ThenInclude(i => i.Ativo)
            .Where(c => c.UsuarioId == usuarioId && c.Ano == ano)
            .OrderByDescending(c => c.Data)
            .FirstOrDefaultAsync();
    }

    public async Task SalvarAsync(CalculoIR calculoIR)
    {
        await _context.CalculosIR.AddAsync(calculoIR);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(Guid id)
    {
        var calculo = await _context.CalculosIR.FindAsync(id);
        if (calculo != null)
        {
            _context.CalculosIR.Remove(calculo);
            await _context.SaveChangesAsync();
        }
    }
}
