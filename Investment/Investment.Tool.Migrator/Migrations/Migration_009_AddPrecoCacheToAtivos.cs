using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251222002)]
public class Migration_009_AddPrecoCacheToAtivos : Migration
{
    public override void Up()
    {
        Alter.Table("Ativos")
            .AddColumn("PrecoAtual").AsDecimal(18, 2).Nullable()
            .AddColumn("PrecoAtualizadoEm").AsDateTimeOffset().Nullable()
            .AddColumn("FonteCotacao").AsString(50).Nullable();

        Create.Index("IX_Ativos_PrecoAtualizadoEm")
            .OnTable("Ativos")
            .OnColumn("PrecoAtualizadoEm").Descending();
    }

    public override void Down()
    {
        Delete.Index("IX_Ativos_PrecoAtualizadoEm").OnTable("Ativos");
        Delete.Column("PrecoAtual").FromTable("Ativos");
        Delete.Column("PrecoAtualizadoEm").FromTable("Ativos");
        Delete.Column("FonteCotacao").FromTable("Ativos");
    }
}
