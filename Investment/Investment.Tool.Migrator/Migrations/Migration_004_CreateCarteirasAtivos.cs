using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251125004)]
public class Migration_004_CreateCarteirasAtivos : Migration
{
    public override void Up()
    {
        Create.Table("CarteirasAtivos")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("CarteiraId").AsInt64().NotNullable()
            .WithColumn("AtivoId").AsInt64().NotNullable();

        Create.ForeignKey("FK_CarteirasAtivos_Carteiras_CarteiraId")
            .FromTable("CarteirasAtivos").ForeignColumn("CarteiraId")
            .ToTable("Carteiras").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("FK_CarteirasAtivos_Ativos_AtivoId")
            .FromTable("CarteirasAtivos").ForeignColumn("AtivoId")
            .ToTable("Ativos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("IX_CarteirasAtivos_CarteiraId_AtivoId")
            .OnTable("CarteirasAtivos")
            .OnColumn("CarteiraId").Ascending()
            .OnColumn("AtivoId").Ascending()
            .WithOptions()
            .Unique();
    }

    public override void Down()
    {
        Delete.Table("CarteirasAtivos");
    }
}
