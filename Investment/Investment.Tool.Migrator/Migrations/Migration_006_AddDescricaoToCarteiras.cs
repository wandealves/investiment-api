using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251218006)]
public class Migration_006_AddDescricaoToCarteiras : Migration
{
    public override void Up()
    {
        Alter.Table("Carteiras")
            .AddColumn("Descricao").AsString(500).Nullable();
    }

    public override void Down()
    {
        Delete.Column("Descricao").FromTable("Carteiras");
    }
}
