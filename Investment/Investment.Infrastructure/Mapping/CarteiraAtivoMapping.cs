using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class CarteiraAtivoMapping: IEntityTypeConfiguration<CarteiraAtivo>
{
    public void Configure(EntityTypeBuilder<CarteiraAtivo> builder)
    {
        builder.ToTable("CarteirasAtivos");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Carteira)
            .WithMany(x => x.CarteirasAtivos)
            .HasForeignKey(x => x.CarteiraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Ativo)
            .WithMany(x => x.CarteirasAtivos)
            .HasForeignKey(x => x.AtivoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Para impedir o mesmo ativo ser duplicado na mesma carteira:
        builder.HasIndex(x => new { x.CarteiraId, x.AtivoId })
            .IsUnique();
    }
}