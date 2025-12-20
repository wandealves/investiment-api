using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(20251220007)]
public class Migration_007_UpdateTipoAtivoToEnum : Migration
{
    public override void Up()
    {
        // Atualizar valores existentes para corresponder aos nomes do enum
        // Mapear variações para os valores corretos do enum
        Execute.Sql(@"
            UPDATE ""Ativos""
            SET ""Tipo"" = CASE
                WHEN ""Tipo"" IN ('Ação', 'Acao', 'ACAO', 'acao') THEN 'Acao'
                WHEN ""Tipo"" IN ('ETF', 'etf') THEN 'ETF'
                WHEN ""Tipo"" IN ('FII', 'fii', 'Fundo Imobiliário') THEN 'FII'
                WHEN ""Tipo"" IN ('Cripto', 'CRIPTO', 'cripto', 'Criptomoeda') THEN 'Cripto'
                WHEN ""Tipo"" IN ('RF', 'rf', 'Renda Fixa') THEN 'RF'
                ELSE 'Acao'  -- Valor padrão para tipos desconhecidos
            END
        ");
    }

    public override void Down()
    {
        // Não há necessidade de reverter, pois os valores continuam sendo strings válidas
    }
}
