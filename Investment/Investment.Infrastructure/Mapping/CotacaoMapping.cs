using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class CotacaoMapping : IEntityTypeConfiguration<Cotacao>
{
    public void Configure(EntityTypeBuilder<Cotacao> builder)
    {
        builder.ToTable("Cotacoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AtivoId)
            .IsRequired();

        builder.Property(x => x.Preco)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DataHora)
            .IsRequired();

        builder.Property(x => x.TipoCotacao)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PrecoAbertura)
            .HasPrecision(18, 2);

        builder.Property(x => x.PrecoMaximo)
            .HasPrecision(18, 2);

        builder.Property(x => x.PrecoMinimo)
            .HasPrecision(18, 2);

        builder.Property(x => x.Volume);

        builder.Property(x => x.Fonte)
            .HasMaxLength(50);

        // Relacionamento com Ativo
        builder.HasOne(x => x.Ativo)
            .WithMany(a => a.Cotacoes)
            .HasForeignKey(x => x.AtivoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => new { x.AtivoId, x.DataHora })
            .HasDatabaseName("IX_Cotacoes_AtivoId_DataHora");

        builder.HasIndex(x => x.DataHora)
            .HasDatabaseName("IX_Cotacoes_DataHora");
    }
}
