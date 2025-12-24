using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251224012)]
public class Migration_012_CreateProventos : Migration
{
    public override void Up()
    {
        Create.Table("Proventos")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("AtivoId").AsInt64().NotNullable()
            .WithColumn("TipoProvento").AsString(20).NotNullable()
            .WithColumn("ValorPorCota").AsDecimal(18, 8).NotNullable()
            .WithColumn("DataCom").AsDateTimeOffset().NotNullable()
            .WithColumn("DataEx").AsDateTimeOffset().Nullable()
            .WithColumn("DataPagamento").AsDateTimeOffset().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable()
            .WithColumn("Observacao").AsString(500).Nullable()
            .WithColumn("CriadoEm").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        // Foreign key para Ativos
        Create.ForeignKey("FK_Proventos_Ativos")
            .FromTable("Proventos").ForeignColumn("AtivoId")
            .ToTable("Ativos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.None);

        // Índices para melhorar performance
        Create.Index("IX_Proventos_AtivoId")
            .OnTable("Proventos")
            .OnColumn("AtivoId");

        Create.Index("IX_Proventos_DataPagamento")
            .OnTable("Proventos")
            .OnColumn("DataPagamento");

        Create.Index("IX_Proventos_Status")
            .OnTable("Proventos")
            .OnColumn("Status");
    }

    public override void Down()
    {
        Delete.Table("Proventos");
    }
}
