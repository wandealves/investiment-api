using Investment.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investment.Infrastructure.Mapping;

public class AtivoMapping: IEntityTypeConfiguration<Ativo>
{
    public void Configure(EntityTypeBuilder<Ativo> builder)
    {
        builder.ToTable("Ativos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Descricao)
            .HasMaxLength(1000);

        // Campos de cotação (cache)
        builder.Property(x => x.PrecoAtual)
            .HasPrecision(18, 2);

        builder.Property(x => x.PrecoAtualizadoEm);

        builder.Property(x => x.FonteCotacao)
            .HasMaxLength(50);

        builder.HasIndex(x => x.Codigo).IsUnique();

        builder.HasIndex(x => x.PrecoAtualizadoEm)
            .HasDatabaseName("IX_Ativos_PrecoAtualizadoEm");
    }  
}