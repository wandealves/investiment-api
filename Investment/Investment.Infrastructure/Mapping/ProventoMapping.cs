using Investment.Domain.Common;
using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class ProventoMapping : IEntityTypeConfiguration<Provento>
{
    public void Configure(EntityTypeBuilder<Provento> builder)
    {
        builder.ToTable("Proventos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoProvento)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.ValorPorCota)
            .IsRequired()
            .HasColumnType("decimal(18,8)");

        builder.Property(x => x.DataCom)
            .IsRequired();

        builder.Property(x => x.DataEx);

        builder.Property(x => x.DataPagamento)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Observacao)
            .HasMaxLength(500);

        builder.Property(x => x.CriadoEm)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Ativo)
            .WithMany()
            .HasForeignKey(x => x.AtivoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transacoes)
            .WithOne(x => x.Provento)
            .HasForeignKey(x => x.ProventoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AtivoId);
        builder.HasIndex(x => x.DataPagamento);
        builder.HasIndex(x => x.Status);
    }
}
