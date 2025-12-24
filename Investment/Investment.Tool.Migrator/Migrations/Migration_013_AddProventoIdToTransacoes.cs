using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251224013)]
public class Migration_013_AddProventoIdToTransacoes : Migration
{
    public override void Up()
    {
        // Adicionar coluna ProventoId (nullable)
        Alter.Table("Transacoes")
            .AddColumn("ProventoId").AsInt64().Nullable();

        // Foreign key para Proventos (opcional)
        Create.ForeignKey("FK_Transacoes_Proventos")
            .FromTable("Transacoes").ForeignColumn("ProventoId")
            .ToTable("Proventos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.SetNull); // Se provento for excluído, ProventoId vira NULL

        // Índice para melhorar performance de queries
        Create.Index("IX_Transacoes_ProventoId")
            .OnTable("Transacoes")
            .OnColumn("ProventoId");
    }

    public override void Down()
    {
        Delete.Index("IX_Transacoes_ProventoId").OnTable("Transacoes");
        Delete.ForeignKey("FK_Transacoes_Proventos").OnTable("Transacoes");
        Delete.Column("ProventoId").FromTable("Transacoes");
    }
}
