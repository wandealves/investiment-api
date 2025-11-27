using Gridify;
using Gridify.EntityFramework;
using Investment.Domain.Entidades;
using Investment.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Repositories;

public class TransacaoRepository(InvestmentDbContext context) : ITransacaoRepository
{
    public async Task<Transacao> SalvarAsync(Transacao transacao)
    {
        if (transacao == null)
            throw new ArgumentNullException(nameof(transacao));
        var carteiraExiste = await context.Carteiras
            .AnyAsync(c => c.Id == transacao.CarteiraId)
            .ConfigureAwait(false);

        if (!carteiraExiste)
            throw new InvalidOperationException($"Carteira com ID '{transacao.CarteiraId}' não encontrada.");
        var ativoExiste = await context.Ativos
            .AnyAsync(a => a.Id == transacao.AtivoId)
            .ConfigureAwait(false);

        if (!ativoExiste)
            throw new InvalidOperationException($"Ativo com ID '{transacao.AtivoId}' não encontrado.");

        await context.Transacoes.AddAsync(transacao).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return transacao;
    }

    public async Task<Transacao> AtualizarAsync(Transacao transacao)
    {
        if (transacao == null)
            throw new ArgumentNullException(nameof(transacao));
        
        var transacaoExistente = await context.Transacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transacao.Id)
            .ConfigureAwait(false);

        if (transacaoExistente == null)
            throw new InvalidOperationException($"Transação com ID '{transacao.Id}' não encontrada.");
        
        var carteiraExiste = await context.Carteiras
            .AnyAsync(c => c.Id == transacao.CarteiraId)
            .ConfigureAwait(false);

        if (!carteiraExiste)
            throw new InvalidOperationException($"Carteira com ID '{transacao.CarteiraId}' não encontrada.");
        
        var ativoExiste = await context.Ativos
            .AnyAsync(a => a.Id == transacao.AtivoId)
            .ConfigureAwait(false);

        if (!ativoExiste)
            throw new InvalidOperationException($"Ativo com ID '{transacao.AtivoId}' não encontrado.");

        context.Transacoes.Update(transacao);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return transacao;
    }

    public async Task<Transacao?> ObterPorIdAsync(Guid id)
    {
        return await context.Transacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterTodosAsync()
    {
        return await context.Transacoes
            .AsNoTracking()
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Paging<Transacao>> ObterAsync(GridifyQuery query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryable = context.Transacoes.AsNoTracking();

        return await queryable
            .GridifyAsync(query)
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorCarteiraIdAsync(long carteiraId)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorAtivoIdAsync(long ativoId)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.AtivoId == ativoId)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorCarteiraEAtivoAsync(long carteiraId, long ativoId)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId && t.AtivoId == ativoId)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.DataTransacao >= inicio && t.DataTransacao <= fim)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorCarteiraEPeriodoAsync(long carteiraId, DateTime inicio, DateTime fim)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId &&
                       t.DataTransacao >= inicio &&
                       t.DataTransacao <= fim)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorTipoAsync(string tipoTransacao)
    {
        if (string.IsNullOrWhiteSpace(tipoTransacao))
            return await ObterTodosAsync().ConfigureAwait(false);

        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.TipoTransacao == tipoTransacao)
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterPorCarteiraTipoEPeriodoAsync(long carteiraId, string tipoTransacao, DateTime inicio, DateTime fim)
    {
        var query = context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId &&
                       t.DataTransacao >= inicio &&
                       t.DataTransacao <= fim);

        if (!string.IsNullOrWhiteSpace(tipoTransacao))
        {
            query = query.Where(t => t.TipoTransacao == tipoTransacao);
        }

        return await query
            .OrderByDescending(t => t.DataTransacao)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Transacao?> ObterComDetalhesAsync(Guid id)
    {
        return await context.Transacoes
            .AsNoTracking()
            .Include(t => t.Carteira)
            .Include(t => t.Ativo)
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<List<Transacao>> ObterUltimasTransacoesAsync(long carteiraId, int quantidade)
    {
        if (quantidade <= 0)
            quantidade = 10;

        return await context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId)
            .OrderByDescending(t => t.DataTransacao)
            .Take(quantidade)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<decimal> CalcularTotalInvestidoPorAtivoAsync(long carteiraId, long ativoId)
    {
        var total = await context.Transacoes
            .AsNoTracking()
            .Where(t => t.CarteiraId == carteiraId && t.AtivoId == ativoId)
            .SumAsync(t => t.Quantidade * t.Preco)
            .ConfigureAwait(false);

        return total;
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var transacao = await context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);

        if (transacao == null)
            return false;

        context.Transacoes.Remove(transacao);
        var resultado = await context.SaveChangesAsync().ConfigureAwait(false);

        return resultado > 0;
    }
}
