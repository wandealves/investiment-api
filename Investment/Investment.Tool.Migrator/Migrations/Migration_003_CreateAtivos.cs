using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251125003)]
public class Migration_003_CreateAtivos : Migration
{
    public override void Up()
    {
        Create.Table("Ativos")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("Nome").AsString(150).NotNullable()
            .WithColumn("Codigo").AsString(20).NotNullable()
            .WithColumn("Tipo").AsString(50).NotNullable()
            .WithColumn("Descricao").AsString(1000).Nullable();

        Create.Index("IX_Ativos_Codigo")
            .OnTable("Ativos")
            .OnColumn("Codigo")
            .Ascending()
            .WithOptions()
            .Unique();
    }

    public override void Down()
    {
        Delete.Table("Ativos");
    }
}
