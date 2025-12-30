using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class CalculoIRMapping : IEntityTypeConfiguration<CalculoIR>
{
    public void Configure(EntityTypeBuilder<CalculoIR> builder)
    {
        builder.ToTable("CalculosIR");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId).IsRequired();
        builder.Property(x => x.Ano).IsRequired(false);
        builder.Property(x => x.Data).IsRequired();

        builder.Property(x => x.Total)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Valor)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.TotalTaxas)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        //builder.Property(x => x.TotalGanhoCapital)
        //    .IsRequired()
        //    .HasColumnType("decimal(18,4)");

        //builder.Property(x => x.TotalIRDevido)
        //    .IsRequired()
        //    .HasColumnType("decimal(18,4)");

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.Ano });
    }
}
