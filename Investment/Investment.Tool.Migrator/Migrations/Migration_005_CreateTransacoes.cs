using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251125005)]
public class Migration_005_CreateTransacoes : Migration
{
    public override void Up()
    {
        Create.Table("Transacoes")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("CarteiraId").AsInt64().NotNullable()
            .WithColumn("AtivoId").AsInt64().NotNullable()
            .WithColumn("Quantidade").AsDecimal(18, 4).NotNullable()
            .WithColumn("Preco").AsDecimal(18, 4).NotNullable()
            .WithColumn("TipoTransacao").AsString(50).NotNullable()
            .WithColumn("DataTransacao").AsDateTimeOffset().NotNullable();

        Create.ForeignKey("FK_Transacoes_Carteiras_CarteiraId")
            .FromTable("Transacoes").ForeignColumn("CarteiraId")
            .ToTable("Carteiras").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.None);

        Create.ForeignKey("FK_Transacoes_Ativos_AtivoId")
            .FromTable("Transacoes").ForeignColumn("AtivoId")
            .ToTable("Ativos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.None);

        Create.Index("IX_Transacoes_CarteiraId_AtivoId_DataTransacao")
            .OnTable("Transacoes")
            .OnColumn("CarteiraId").Ascending()
            .OnColumn("AtivoId").Ascending()
            .OnColumn("DataTransacao").Ascending();
    }

    public override void Down()
    {
        Delete.Table("Transacoes");
    }
}
