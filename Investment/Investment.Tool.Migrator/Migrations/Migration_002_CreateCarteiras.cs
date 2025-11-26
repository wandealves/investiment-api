using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251125002)]
public class Migration_002_CreateCarteiras : Migration
{
    public override void Up()
    {
        Create.Table("Carteiras")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("UsuarioId").AsGuid().NotNullable()
            .WithColumn("Nome").AsString(150).NotNullable()
            .WithColumn("CriadaEm").AsDateTimeOffset().NotNullable();

        Create.ForeignKey("FK_Carteiras_Usuarios_UsuarioId")
            .FromTable("Carteiras").ForeignColumn("UsuarioId")
            .ToTable("Usuarios").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("Carteiras");
    }
}
