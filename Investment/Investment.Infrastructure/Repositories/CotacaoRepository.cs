using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class CotacaoRepository : ICotacaoRepository
{
    private readonly InvestmentDbContext _context;

    public CotacaoRepository(InvestmentDbContext context)
    {
        _context = context;
    }

    public async Task<Cotacao> SalvarAsync(Cotacao cotacao)
    {
        await _context.Cotacoes.AddAsync(cotacao);
        await _context.SaveChangesAsync();
        return cotacao;
    }

    public async Task<Cotacao?> ObterPorIdAsync(long id)
    {
        return await _context.Cotacoes
            .Include(c => c.Ativo)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cotacao>> ObterPorAtivoIdAsync(long ativoId)
    {
        return await _context.Cotacoes
            .Where(c => c.AtivoId == ativoId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();
    }

    public async Task<List<Cotacao>> ObterPorAtivoEPeriodoAsync(long ativoId, DateTimeOffset inicio, DateTimeOffset fim)
    {
        return await _context.Cotacoes
            .Where(c => c.AtivoId == ativoId && c.DataHora >= inicio && c.DataHora <= fim)
            .OrderBy(c => c.DataHora)
            .ToListAsync();
    }

    public async Task<Cotacao?> ObterUltimaCotacaoAsync(long ativoId)
    {
        return await _context.Cotacoes
            .Where(c => c.AtivoId == ativoId)
            .OrderByDescending(c => c.DataHora)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var cotacao = await _context.Cotacoes.FindAsync(id);
        if (cotacao == null)
            return false;

        _context.Cotacoes.Remove(cotacao);
        await _context.SaveChangesAsync();
        return true;
    }
}
