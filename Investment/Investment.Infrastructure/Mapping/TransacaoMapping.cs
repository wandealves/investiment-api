using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class TransacaoMapping: IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("Transacoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Preco)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.TipoTransacao)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DataTransacao)
            .IsRequired();

        builder.HasOne(x => x.Carteira)
            .WithMany(x => x.Transacoes)
            .HasForeignKey(x => x.CarteiraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ativo)
            .WithMany(x => x.Transacoes)
            .HasForeignKey(x => x.AtivoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Provento)
            .WithMany(x => x.Transacoes)
            .HasForeignKey(x => x.ProventoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.CarteiraId, x.AtivoId, x.DataTransacao });
        builder.HasIndex(x => x.ProventoId);
    }
}