using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class ItemCalculoIRMapping : IEntityTypeConfiguration<ItemCalculoIR>
{
    public void Configure(EntityTypeBuilder<ItemCalculoIR> builder)
    {
        builder.ToTable("ItensCalculoIR");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Total)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.PrecoMedio)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.PrecoAtual)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Rendimento)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.TaxasRateadas)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Data)
            .IsRequired(false);

        builder.HasOne(x => x.CalculoIR)
            .WithMany(x => x.Itens)
            .HasForeignKey(x => x.CalculoIRId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Ativo)
            .WithMany()
            .HasForeignKey(x => x.AtivoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CalculoIRId);
    }
}