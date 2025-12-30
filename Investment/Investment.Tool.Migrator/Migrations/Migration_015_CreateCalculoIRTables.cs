using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(202512301922)]
public class Migration_015_CreateCalculoIRTables : Migration
{
    public override void Up()
    {
        // Tabela principal de cálculo
        Create.Table("CalculosIR")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("UsuarioId").AsGuid().NotNullable()
            .WithColumn("CarteiraId").AsInt64().NotNullable()
            .WithColumn("Ano").AsInt32().Nullable()  // null = "todos os anos"
            .WithColumn("Data").AsDateTimeOffset().NotNullable()
            .WithColumn("Total").AsDecimal(18, 4).NotNullable()
            .WithColumn("Valor").AsDecimal(18, 4).Nullable()
            .WithColumn("TotalTaxas").AsDecimal(18, 4).NotNullable();

        Create.ForeignKey("FK_CalculosIR_Usuarios")
            .FromTable("CalculosIR").ForeignColumn("UsuarioId")
            .ToTable("Usuarios").PrimaryColumn("Id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("FK_CalculosIR_Carteiras")
            .FromTable("CalculosIR").ForeignColumn("CarteiraId")
            .ToTable("Carteiras").PrimaryColumn("Id")
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
            .WithColumn("Total").AsDecimal(18, 4).NotNullable()
            .WithColumn("PrecoMedio").AsDecimal(18, 4).NotNullable()
            .WithColumn("PrecoAtual").AsDecimal(18, 4).Nullable()
            .WithColumn("Rendimento").AsDecimal(18, 4).Nullable()
            .WithColumn("TaxasRateadas").AsDecimal(18, 4).NotNullable();

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
