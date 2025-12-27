using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(14)]
public class Migration_014_AddTaxaToTransacoes : Migration
{
    public override void Up()
    {
        Alter.Table("Transacoes")
            .AddColumn("Taxa").AsDecimal(18, 4).NotNullable().WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Column("Taxa").FromTable("Transacoes");
    }
}
