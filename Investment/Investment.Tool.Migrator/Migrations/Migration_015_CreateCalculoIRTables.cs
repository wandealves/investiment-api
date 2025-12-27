using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(15)]
public class Migration_015_CreateCalculoIRTables : Migration
{
    public override void Up()
    {
        // Tabela principal de cálculo
        Create.Table("CalculosIR")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("UsuarioId").AsGuid().NotNullable()
            .WithColumn("Ano").AsInt32().Nullable()  // null = "todos os anos"
            .WithColumn("DataCalculo").AsDateTimeOffset().NotNullable()
            .WithColumn("ValorTotalInvestido").AsDecimal(18, 4).NotNullable()
            .WithColumn("ValorTotalAtual").AsDecimal(18, 4).Nullable()
            .WithColumn("TotalTaxasRateadas").AsDecimal(18, 4).NotNullable()
            .WithColumn("TotalGanhoCapital").AsDecimal(18, 4).NotNullable()
            .WithColumn("TotalIRDevido").AsDecimal(18, 4).NotNullable();

        Create.ForeignKey("FK_CalculosIR_Usuarios")
            .FromTable("CalculosIR").ForeignColumn("UsuarioId")
            .ToTable("Usuarios").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("IX_CalculosIR_UsuarioId_Ano")
            .OnTable("CalculosIR")
            .OnColumn("UsuarioId").Ascending()
            .OnColumn("Ano").Ascending();

        // Tabela de itens por ativo
        Create.Table("ItensCalculoIR")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("CalculoIRId").AsGuid().NotNullable()
            .WithColumn("AtivoId").AsInt64().NotNullable()
            .WithColumn("Quantidade").AsDecimal(18, 4).NotNullable()
            .WithColumn("TotalInvestido").AsDecimal(18, 4).NotNullable()
            .WithColumn("PrecoMedio").AsDecimal(18, 4).NotNullable()
            .WithColumn("PrecoAtual").AsDecimal(18, 4).Nullable()
            .WithColumn("Rendimento").AsDecimal(18, 4).Nullable()
            .WithColumn("TaxasRateadas").AsDecimal(18, 4).NotNullable()
            .WithColumn("GanhoCapital").AsDecimal(18, 4).NotNullable()
            .WithColumn("IRDevido").AsDecimal(18, 4).NotNullable()
            .WithColumn("AliquotaIR").AsDecimal(5, 2).NotNullable();  // 15.00, 20.00

        Create.ForeignKey("FK_ItensCalculoIR_CalculosIR")
            .FromTable("ItensCalculoIR").ForeignColumn("CalculoIRId")
            .ToTable("CalculosIR").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("FK_ItensCalculoIR_Ativos")
            .FromTable("ItensCalculoIR").ForeignColumn("AtivoId")
            .ToTable("Ativos").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.None);

        Create.Index("IX_ItensCalculoIR_CalculoIRId")
            .OnTable("ItensCalculoIR")
            .OnColumn("CalculoIRId").Ascending();
    }

    public override void Down()
    {
        Delete.Table("ItensCalculoIR");
        Delete.Table("CalculosIR");
    }
}
