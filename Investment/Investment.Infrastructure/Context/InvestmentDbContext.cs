using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Investment.Infrastructure.Context;

public class InvestmentDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Carteira> Carteiras { get; set; }
    public DbSet<Ativo> Ativos { get; set; }
    public DbSet<CarteiraAtivo> CarteirasAtivos { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<Cotacao> Cotacoes { get; set; }
    public DbSet<Provento> Proventos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvestmentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}