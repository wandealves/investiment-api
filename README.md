# Investment API

Sistema completo de gestão de carteiras de investimentos desenvolvido em .NET 10.0 com PostgreSQL.

## Sobre o Projeto

Investment API é uma solução backend robusta para gerenciamento de portfólios de investimentos, oferecendo recursos avançados de análise financeira, importação de notas de corretagem e geração de relatórios detalhados.

### Principais Funcionalidades

- **Autenticação e Autorização**: JWT Bearer Token com BCrypt para hash de senhas
- **Gestão de Carteiras**: Criação e gerenciamento de múltiplas carteiras por usuário
- **Controle de Ativos**: Suporte a ações, FIIs, ETFs, BDRs, renda fixa e criptomoedas
- **Transações Completas**: 7 tipos de operações (compra, venda, dividendos, JCP, bonificações, splits, grupamentos)
- **Importação de PDFs**: Parsing automático de notas de corretagem (Clear e XP)
- **Posição Consolidada**: Cálculo de preço médio usando algoritmo WAC (Weighted Average Cost)
- **Métricas Financeiras**: IRR (Internal Rate of Return) e TWR (Time-Weighted Return)
- **Relatórios**: Exportação em PDF e Excel com análises detalhadas

## Tecnologias Utilizadas

### Backend
- **.NET 10.0** - Framework principal
- **ASP.NET Core Minimal API** - Arquitetura de endpoints
- **Entity Framework Core** - ORM para acesso a dados
- **PostgreSQL** - Banco de dados relacional
- **FluentMigrator** - Gerenciamento de migrações de schema

### Segurança
- **JWT Bearer Authentication** - Autenticação stateless
- **BCrypt.Net** - Hashing de senhas (workFactor 12)

### Bibliotecas Principais
- **Gridify** - Paginação, filtros e ordenação
- **iText7** - Parsing de PDFs de corretoras
- **QuestPDF** - Geração de relatórios em PDF
- **ClosedXML** - Geração de planilhas Excel

### Ferramentas
- **Scalar** - Documentação interativa da API (OpenAPI/Swagger)

## Arquitetura

O projeto segue **Clean Architecture** com 4 camadas distintas:

```
Investment/
├── Investment.Api/              # Camada de apresentação (endpoints)
├── Investment.Application/      # Lógica de negócio (services, DTOs)
├── Investment.Domain/          # Entidades de domínio e value objects
├── Investment.Infrastructure/  # Acesso a dados (repositories, EF Core)
└── Investment.Tool.Migrator/   # Ferramenta de migração de banco
```

### Padrões Implementados

- **Result Pattern**: Tratamento padronizado de erros
- **Repository Pattern**: Abstração de acesso a dados
- **Strategy Pattern**: Parsers de PDF modulares
- **Dependency Injection**: Inversão de controle nativa do .NET
- **DTO Pattern**: Separação entre entidades de domínio e API

## Pré-requisitos

- .NET SDK 10.0 ou superior
- PostgreSQL 14 ou superior
- Git

## Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/investment-api.git
cd investment-api
```

### 2. Configure o banco de dados

Crie um banco de dados PostgreSQL:

```sql
CREATE DATABASE InvestmentDb;
```

### 3. Configure a connection string

Edite `Investment/Investment.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=InvestmentDb;Username=postgres;Password=sua_senha"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-minimo-32-caracteres",
    "Issuer": "InvestmentApi",
    "Audience": "InvestmentApiClients",
    "ExpiresInHours": 24
  }
}
```

### 4. Execute as migrações

```bash
cd Investment/Investment.Tool.Migrator
./migrate.sh
```

Ou usando dotnet CLI:

```bash
cd Investment/Investment.Tool.Migrator
dotnet run
```

### 5. Restaure as dependências

```bash
cd Investment
dotnet restore
```

### 6. Execute o projeto

```bash
cd Investment/Investment.Api
dotnet run
```

A API estará disponível em: `https://localhost:5001`

## Documentação da API

Acesse a documentação interativa (Scalar UI) em:

```
https://localhost:5001/scalar/v1
```

## Endpoints Principais

### Autenticação

```http
POST /api/v1/auth/register          # Registrar novo usuário
POST /api/v1/auth/login             # Login (retorna JWT)
POST /api/v1/auth/alterar-senha     # Alterar senha (requer auth)
GET  /api/v1/auth/me                # Dados do usuário autenticado
```

### Usuários

```http
GET    /api/v1/usuarios/{id}              # Obter usuário
GET    /api/v1/usuarios/{id}/carteiras    # Usuário com carteiras
PUT    /api/v1/usuarios/{id}              # Atualizar usuário
DELETE /api/v1/usuarios/{id}              # Excluir usuário
```

### Carteiras

```http
GET    /api/v1/carteiras                  # Listar carteiras do usuário
GET    /api/v1/carteiras/{id}             # Obter carteira
GET    /api/v1/carteiras/{id}/detalhes    # Carteira com detalhes completos
POST   /api/v1/carteiras                  # Criar carteira
PUT    /api/v1/carteiras/{id}             # Atualizar carteira
DELETE /api/v1/carteiras/{id}             # Excluir carteira
```

### Ativos

```http
GET    /api/v1/ativos                     # Listar ativos (com paginação/filtros)
GET    /api/v1/ativos/{id}                # Obter ativo
POST   /api/v1/ativos                     # Criar ativo
PUT    /api/v1/ativos/{id}                # Atualizar ativo
DELETE /api/v1/ativos/{id}                # Excluir ativo
```

### Transações

```http
GET    /api/v1/transacoes/{id}                          # Obter transação
GET    /api/v1/carteiras/{id}/transacoes                # Listar transações da carteira
GET    /api/v1/carteiras/{id}/transacoes/periodo        # Filtrar por período
POST   /api/v1/transacoes                               # Criar transação
PUT    /api/v1/transacoes/{id}                          # Atualizar transação
DELETE /api/v1/transacoes/{id}                          # Excluir transação
```

### Posição Consolidada

```http
GET /api/v1/carteiras/{id}/posicao              # Posição consolidada da carteira
GET /api/v1/carteiras/{id}/posicao/{ativoId}    # Posição de um ativo específico
GET /api/v1/posicoes                            # Todas as posições do usuário
```

### Importação de PDFs

```http
POST /api/v1/importacao/preview     # Preview da importação (não salva)
POST /api/v1/importacao/confirmar   # Importar e salvar transações
```

**Exemplo de request (multipart/form-data):**

```
carteiraId: 1
corretoraTipo: Clear
file: nota_corretagem.pdf
```

### Relatórios

```http
GET /api/v1/relatorios/rentabilidade/{id}?inicio=&fim=    # JSON com IRR, TWR
GET /api/v1/relatorios/proventos/{id}?inicio=&fim=        # JSON com proventos
GET /api/v1/relatorios/{id}/pdf?inicio=&fim=              # Download PDF
GET /api/v1/relatorios/{id}/excel?inicio=&fim=            # Download Excel
```

## Exemplos de Uso

### 1. Registrar e Autenticar

```bash
# Registrar usuário
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "email": "joao@example.com",
    "senha": "Senha@123"
  }'

# Login
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "senha": "Senha@123"
  }'
```

### 2. Criar Carteira

```bash
curl -X POST https://localhost:5001/api/v1/carteiras \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Carteira Long Term",
    "descricao": "Investimentos de longo prazo"
  }'
```

### 3. Registrar Transação

```bash
curl -X POST https://localhost:5001/api/v1/transacoes \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "carteiraId": 1,
    "ativoId": 5,
    "quantidade": 100,
    "preco": 28.50,
    "tipoTransacao": "Compra",
    "dataTransacao": "2025-12-10T10:00:00Z"
  }'
```

### 4. Consultar Posição

```bash
curl -X GET "https://localhost:5001/api/v1/carteiras/1/posicao" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

### 5. Gerar Relatório

```bash
curl -X GET "https://localhost:5001/api/v1/relatorios/1/pdf?inicio=2025-01-01&fim=2025-12-31" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  --output relatorio.pdf
```

## Algoritmo de Cálculo de Posição

### Weighted Average Cost (WAC)

O sistema utiliza o método WAC para calcular o preço médio de compra:

```
PrecoMedio = ((QtdAtual × PrecoMedio) + (QtdCompra × PrecoCompra)) / (QtdAtual + QtdCompra)
```

### Tratamento de Eventos Corporativos

- **Venda**: Reduz quantidade, mantém preço médio
- **Dividendo/JCP**: Acumula em proventos separados
- **Bonificação**: Aumenta quantidade, recalcula preço médio
- **Split (2:1)**: Quantidade × 2, Preço ÷ 2
- **Grupamento (10:1)**: Quantidade ÷ 10, Preço × 10

## Métricas Financeiras

### IRR (Internal Rate of Return)

Calculado usando método **Newton-Raphson** para encontrar a taxa onde NPV = 0:

```
NPV = Σ (CashFlow_i / (1 + IRR)^((Date_i - Date_0) / 365))
```

- Máximo de 100 iterações
- Tolerância: 0.0001
- Retorno anualizado em percentual

### TWR (Time-Weighted Return)

Elimina o efeito do timing de aportes e resgates:

```
TWR = Π ((Ending_Value + Withdrawals) / (Beginning_Value + Deposits)) - 1
```

## Estrutura do Banco de Dados

### Principais Tabelas

- **Usuarios**: Contas de usuário (GUID PK)
- **Carteiras**: Portfolios de investimento (bigint PK, FK para Usuarios)
- **Ativos**: Catálogo de ativos (bigint PK, unique Codigo)
- **Transacoes**: Operações financeiras (GUID PK, RESTRICT delete)
- **CarteirasAtivos**: Relacionamento M:N (composite unique index)

## Segurança

### Implementações de Segurança

- Todos os endpoints (exceto /login e /register) exigem autenticação JWT
- Tokens expiram em 24 horas
- Senhas hashadas com BCrypt (workFactor=12)
- Validação de complexidade de senha: mínimo 8 caracteres, maiúscula, minúscula, dígito e caractere especial
- Email único por usuário
- Verificação de ownership em todas as operações
- HTTPS enforçado via UseHttpsRedirection
- Upload de arquivos limitado a 5MB e apenas PDFs
- Validação de magic bytes em PDFs (%PDF)
- SQL injection prevenido via parametrização do EF Core

## Desenvolvimento

### Executar Testes

```bash
cd Investment
dotnet test
```

### Build para Produção

```bash
cd Investment
dotnet publish -c Release -o ./publish
```

### Adicionar Nova Migração

```bash
cd Investment/Investment.Tool.Migrator
# Edite os arquivos em Migrations/
./migrate.sh
```

### Rollback de Migração

```bash
cd Investment/Investment.Tool.Migrator
./migrate.sh down
```

## Comandos Úteis

```bash
# Restaurar dependências
dotnet restore

# Compilar solução
dotnet build

# Executar API
dotnet run --project Investment.Api

# Limpar build
dotnet clean

# Verificar versão do .NET
dotnet --version
```

## Contribuindo

Contribuições são bem-vindas! Por favor:

1. Faça fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

### Padrões de Código

- Seguir convenções C# e .NET
- Usar async/await para operações I/O
- Manter Result pattern para retorno de operações
- Adicionar validações na camada de serviço
- Documentar endpoints com WithDescription()
- Manter ownership validation em todas as operações

## Roadmap

### Próximas Features

- [ ] Testes automatizados (unitários e integração)
- [ ] Integração com APIs de cotação (B3, Alpha Vantage)
- [ ] Notificações de dividendos
- [ ] Comparação de carteiras
- [ ] Benchmarking (IBOV, CDI)
- [ ] Cache com Redis
- [ ] Rate limiting
- [ ] Health checks
- [ ] Logging estruturado (Serilog)
- [ ] Containerização Docker
- [ ] CI/CD pipeline

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## Suporte

Para reportar bugs ou solicitar features, abra uma issue em:
https://github.com/seu-usuario/investment-api/issues

## Autores

- **Seu Nome** - *Desenvolvimento inicial*

## Agradecimentos

- Anthropic Claude para assistência no desenvolvimento
- Comunidade .NET
- Contribuidores do projeto
