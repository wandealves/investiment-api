using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251125001)]
public class Migration_001_CreateUsuarios : Migration
{
    public override void Up()
    {
        Create.Table("Usuarios")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Nome").AsString(150).NotNullable()
            .WithColumn("Email").AsString(200).NotNullable()
            .WithColumn("SenhaHash").AsString(500).NotNullable()
            .WithColumn("CriadoEm").AsDateTimeOffset().NotNullable();

        Create.Index("IX_Usuarios_Email")
            .OnTable("Usuarios")
            .OnColumn("Email")
            .Ascending()
            .WithOptions()
            .Unique();
    }

    public override void Down()
    {
        Delete.Table("Usuarios");
    }
}
