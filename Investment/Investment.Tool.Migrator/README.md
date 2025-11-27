# Investment.Tool.Migrator

Ferramenta de migração de banco de dados para o sistema Investment usando FluentMigrator e PostgreSQL.

## Descrição

Este projeto é responsável por gerenciar o schema do banco de dados da aplicação Investment através de migrations versionadas. Utiliza **FluentMigrator** para criar e manter a estrutura do banco de dados PostgreSQL, garantindo que todas as alterações sejam rastreáveis e reversíveis.

## Tecnologias

- **.NET 10.0**
- **FluentMigrator 7.1.0** - Framework de migrations
- **PostgreSQL** - Banco de dados relacional (via Npgsql)

## Estrutura do Banco de Dados

O schema completo inclui 5 tabelas principais:

### 1. Usuarios
- Armazena informações dos usuários do sistema
- Campos: Id (GUID), Nome, Email (único), SenhaHash, CriadoEm
- Índice único no campo Email

### 2. Carteiras
- Gerencia as carteiras de investimento dos usuários
- Campos: Id (bigint), UsuarioId (FK), Nome, CriadaEm
- Relacionamento: N:1 com Usuarios (CASCADE)

### 3. Ativos
- Catálogo de ativos disponíveis (ações, ETFs, FIIs, criptomoedas, etc.)
- Campos: Id (bigint), Nome, Codigo (único), Tipo, Descricao
- Índice único no campo Codigo

### 4. CarteirasAtivos
- Tabela de junção entre Carteiras e Ativos
- Campos: Id (bigint), CarteiraId (FK), AtivoId (FK)
- Índice único composto (CarteiraId, AtivoId) - previne duplicatas

### 5. Transacoes
- Registra todas as transações financeiras
- Campos: Id (GUID), CarteiraId (FK), AtivoId (FK), Quantidade, Preco, TipoTransacao, DataTransacao
- Relacionamentos com DELETE RESTRICT para preservar histórico
- Índice composto (CarteiraId, AtivoId, DataTransacao)

## Pré-requisitos

- .NET SDK 10.0 ou superior
- PostgreSQL 12+ instalado e configurado
- Acesso ao banco de dados com permissões de DDL

## Configuração

### 1. Connection String

Edite o arquivo `appsettings.json` com suas credenciais do PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=investimentos;Username=seu_usuario;Password=sua_senha;"
  }
}
```

### 2. Restaurar Dependências

```bash
dotnet restore
```

### 3. Build do Projeto

```bash
dotnet build
```

## Como Usar

### Opção 1: Script Shell (Recomendado)

```bash
# Aplicar todas as migrations
./migrate.sh

# Reverter todas as migrations
./migrate.sh down

# Ver ajuda
./migrate.sh help
```

### Opção 2: .NET CLI

```bash
# Aplicar migrations
dotnet run

# Reverter migrations
dotnet run down
```

## Migrations Disponíveis

| Versão | Nome | Descrição |
|--------|------|-----------|
| 20251125001 | CreateUsuarios | Cria tabela de usuários com índice único no email |
| 20251125002 | CreateCarteiras | Cria tabela de carteiras vinculadas aos usuários |
| 20251125003 | CreateAtivos | Cria catálogo de ativos com código único |
| 20251125004 | CreateCarteirasAtivos | Tabela de relacionamento carteira-ativo |
| 20251125005 | CreateTransacoes | Registros de transações financeiras |

## Estrutura de Arquivos

```
Investment.Tool.Migrator/
├── Migrations/
│   ├── Migration_001_CreateUsuarios.cs
│   ├── Migration_002_CreateCarteiras.cs
│   ├── Migration_003_CreateAtivos.cs
│   ├── Migration_004_CreateCarteirasAtivos.cs
│   └── Migration_005_CreateTransacoes.cs
├── appsettings.json
├── Program.cs
├── migrate.sh
├── Investment.Tool.Migrator.csproj
└── README.md
```

## Adicionando Novas Migrations

1. Crie um novo arquivo em `Migrations/` seguindo o padrão:

```csharp
using FluentMigrator;

namespace Investment.Tool.Migrator.Migrations;

[Migration(YYYYMMDDXXX)] // Incrementar o número da versão
public class Migration_XXX_DescricaoDaMudanca : Migration
{
    public override void Up()
    {
        // Código para aplicar a migration
        Create.Table("MinhaTabela")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("Nome").AsString(100).NotNullable();
    }

    public override void Down()
    {
        // Código para reverter a migration
        Delete.Table("MinhaTabela");
    }
}
```

2. Execute o migrator para aplicar a nova migration:

```bash
./migrate.sh
```

## Troubleshooting

### Erro: "Could not load file or assembly 'Npgsql'"

**Solução:** Execute `dotnet restore` e `dotnet build`

### Erro: "Connection refused"

**Solução:** Verifique se o PostgreSQL está rodando e se as credenciais no `appsettings.json` estão corretas

### Erro: "Version table already exists"

**Solução:** A tabela de controle de versão (`VersionInfo`) já existe. Isso é normal em execuções subsequentes.

### Como verificar quais migrations foram aplicadas

Conecte-se ao banco e consulte:

```sql
SELECT * FROM "VersionInfo" ORDER BY "Version";
```

## Políticas de Delete

- **CASCADE**: Quando um Usuário é deletado, suas Carteiras são removidas automaticamente, assim como os relacionamentos em CarteirasAtivos
- **RESTRICT**: Transações não podem ser deletadas se houver referências ativas, preservando o histórico financeiro

## Integração com EF Core

Este projeto trabalha em conjunto com o **Investment.Infrastructure**, que utiliza Entity Framework Core como ORM. As migrations do FluentMigrator garantem que o schema do banco esteja sincronizado com os mapeamentos do EF Core.

## Licença

Este projeto faz parte do sistema Investment.

## Suporte

Para problemas ou dúvidas, consulte a documentação do [FluentMigrator](https://fluentmigrator.github.io/).
