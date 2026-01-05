using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(202601052005)]
public class Migration_016_AddColumnDataToItensCalculoIR : Migration
{
    public override void Up()
    {
        Alter.Table("ItensCalculoIR")
            .AddColumn("Data").AsDateTimeOffset().Nullable().WithDefault(SystemMethods.CurrentDateTimeOffset);
        
    }
    
    public override void Down()
    {
        Delete.Column("Data").FromTable("ItensCalculoIR");
    }
}
