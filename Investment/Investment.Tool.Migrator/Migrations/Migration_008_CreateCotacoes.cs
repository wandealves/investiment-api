using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251222001)]
public class Migration_008_CreateCotacoes : Migration
{
    public override void Up()
    {
        Create.Table("Cotacoes")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("AtivoId").AsInt64().NotNullable()
            .WithColumn("Preco").AsDecimal(18, 2).NotNullable()
            .WithColumn("DataHora").AsDateTimeOffset().NotNullable()
            .WithColumn("TipoCotacao").AsString(20).NotNullable()
            .WithColumn("PrecoAbertura").AsDecimal(18, 2).Nullable()
            .WithColumn("PrecoMaximo").AsDecimal(18, 2).Nullable()
            .WithColumn("PrecoMinimo").AsDecimal(18, 2).Nullable()
            .WithColumn("Volume").AsInt64().Nullable()
            .WithColumn("Fonte").AsString(50).Nullable();

        Create.ForeignKey("FK_Cotacoes_Ativos")
            .FromTable("Cotacoes").ForeignColumn("AtivoId")
            .ToTable("Ativos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("IX_Cotacoes_AtivoId_DataHora")
            .OnTable("Cotacoes")
            .OnColumn("AtivoId").Ascending()
            .OnColumn("DataHora").Descending();

        Create.Index("IX_Cotacoes_DataHora")
            .OnTable("Cotacoes")
            .OnColumn("DataHora").Descending();
    }

    public override void Down()
    {
        Delete.Table("Cotacoes");
    }
}
