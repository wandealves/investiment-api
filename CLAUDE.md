# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Investment API is a .NET 10.0 investment portfolio management system using PostgreSQL for data persistence. The application follows a clean architecture pattern with distinct layers for API, Application, Domain, and Infrastructure.

## Architecture

The solution uses a **4-layer clean architecture**:

1. **Investment.Api** - ASP.NET Core Minimal API entry point
   - Uses endpoint-based routing (pattern: `*Endpoint.cs` files with static `Registrar*Endpoints` methods)
   - Returns standardized JSON responses with `{ success, data, errors, validationErrors }` structure
   - Scalar UI for API documentation (BluePlanet theme, dark mode)

2. **Investment.Application** - Business logic and service layer
   - Services implement business rules and validation
   - DTOs for request/response models (separate from domain entities)
   - Mappers for DTO ↔ Entity conversion
   - Uses **Result pattern** (`Result<T>`) for operation outcomes with error tracking

3. **Investment.Domain** - Core domain entities and business models
   - Pure domain entities with navigation properties
   - `Result` and `Result<T>` classes for standardized error handling
   - Entities: Usuario, Carteira, Ativo, CarteiraAtivo, Transacao

4. **Investment.Infrastructure** - Data access and external concerns
   - EF Core DbContext with PostgreSQL provider
   - Repository pattern for data access
   - Entity configurations using Fluent API (in `Mapping/` folder)
   - All repositories use async operations

5. **Investment.Tool.Migrator** - Database migration tool (separate from main API)
   - FluentMigrator for version-controlled schema management
   - Runs independently via shell script or dotnet CLI

## Database Schema

Core tables managed via FluentMigrator:
- **Usuarios**: User accounts (GUID PK)
- **Carteiras**: Investment portfolios (bigint PK, FK to Usuarios with CASCADE delete)
- **Ativos**: Asset catalog - stocks, ETFs, REITs, crypto (bigint PK, unique Codigo)
- **CarteirasAtivos**: Many-to-many join table (unique composite index on CarteiraId + AtivoId)
- **Transacoes**: Financial transactions (GUID PK, RESTRICT delete to preserve history)

## Development Commands

### Build and Run
```bash
# Build entire solution
dotnet build Investment/Investment.sln

# Run API (from Investment/Investment.Api/)
dotnet run

# Restore dependencies
dotnet restore Investment/Investment.sln
```

### Database Migrations
```bash
# Apply all pending migrations
cd Investment/Investment.Tool.Migrator
./migrate.sh

# Rollback all migrations
./migrate.sh down

# Alternative: using dotnet CLI directly
dotnet run                    # apply migrations
dotnet run down              # rollback migrations
```

### Configuration
Connection string in `Investment/Investment.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=InvestmentDb;Username=postgres;Password=postgres"
  }
}
```

## Key Patterns and Conventions

### Result Pattern
All service methods return `Result` or `Result<T>` which encapsulates:
- `IsSuccess` / `IsFailure` flags
- `Errors` - list of general error messages
- `ValidationErrors` - dictionary of field-specific validation errors
- `Data` - the successful result payload (for `Result<T>`)

### Endpoint Registration
Endpoints are grouped by domain entity in static classes:
```csharp
public static class AtivoEndpoint
{
    public static void RegistrarAtivoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ativos")
            .WithName("Ativos")
            .WithTags("Ativos");
        // ... endpoint mappings
    }
}
```

Register in `Program.cs` via extension method: `app.RegistrarAtivoEndpoints();`

### API Response Format
All endpoints return consistent JSON structure:
```json
{
  "success": true,
  "data": { ... },
  "errors": [],
  "validationErrors": {}
}
```

### Query and Filtering
The API uses **Gridify** for pagination, filtering, and sorting on list endpoints:
- Endpoints accept `GridifyQuery` parameter
- Repositories return `Paging<T>` results
- Example: `GET /api/v1/ativos?page=1&pageSize=20&orderBy=Nome&filter=Tipo=Acao`

### Repository Pattern
- All repositories are async and return `Task<T>`
- Interface/implementation pairs (e.g., `IAtivoRepository` / `AtivoRepository`)
- Repositories registered as scoped services in DI container
- Common operations: `ObterPorIdAsync`, `SalvarAsync`, `AtualizarAsync`, `ExcluirAsync`

### Entity Configuration
EF Core mappings in `Investment.Infrastructure/Mapping/`:
- Use Fluent API (not data annotations)
- Applied automatically via `modelBuilder.ApplyConfigurationsFromAssembly()`
- Table names match FluentMigrator schema exactly

## Important Notes

- **Migrations**: Always use Investment.Tool.Migrator for schema changes. Do NOT use EF Core migrations.
- **Validation**: Service layer validates requests and returns validation errors in `Result<T>.ValidationErrors` dictionary
- **Dependency Injection**: All services and repositories registered in `Program.cs` as scoped
- **Async/Await**: All I/O operations (DB, file, network) must be async
- **API Versioning**: Currently using `/api/v1/` prefix for all routes
- **Authentication**: JWT Bearer Token authentication with BCrypt password hashing

---

## Implementation Status

### ✅ FASE 1: AUTENTICAÇÃO JWT (COMPLETO)

**Objetivo**: Implementar sistema de autenticação seguro com JWT

**Status**: ✅ Concluído

**Implementado**:
- ✅ Pacotes NuGet: `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`
- ✅ DTOs: `LoginRequest`, `RegisterRequest`, `AuthResponse`, `UsuarioResponse`
- ✅ Services: `ITokenService` / `TokenService`, `IAuthService` / `AuthService`
- ✅ Endpoints `/api/v1/auth`:
  - POST `/register` - Registro de novo usuário (público)
  - POST `/login` - Login com email/senha (público)
  - POST `/alterar-senha` - Alteração de senha (protegido)
  - GET `/me` - Dados do usuário autenticado (protegido)
- ✅ Configuração JWT no `Program.cs` com middleware de autenticação/autorização
- ✅ `HttpContextExtensions.GetUsuarioId()` helper para extrair usuário autenticado
- ✅ Configurações JWT no `appsettings.json`

**Segurança**:
- ✅ Hash de senhas com BCrypt (workFactor=12)
- ✅ Validação de senha: min 8 chars, maiúscula, minúscula, dígito, caractere especial
- ✅ Email único no banco de dados
- ✅ Tokens JWT com expiração de 24 horas
- ✅ Endpoints protegidos com `[Authorize]`

---

### ✅ FASE 2: SERVIÇOS CRUD (COMPLETO)

**Objetivo**: Implementar serviços completos para Usuario, Carteira e Transacao

**Status**: ✅ Concluído

**Implementado**:
- ✅ **UsuarioService**: CRUD de usuários com autorização por ownership
  - DTOs: `UsuarioRequest`, `UsuarioResponse`, `UsuarioComCarteirasResponse`
  - Mapper: `UsuarioMapper`
  - Service: `IUsuarioService` / `UsuarioService`
  - Endpoints `/api/v1/usuarios`:
    - GET `/{id}` - Obter usuário por ID
    - GET `/{id}/carteiras` - Obter usuário com carteiras
    - PUT `/{id}` - Atualizar usuário (nome e email)
    - DELETE `/{id}` - Excluir usuário
  - Regra: Usuários só acessam próprios dados (verificação id == usuarioAutenticadoId)

- ✅ **CarteiraService**: Gestão de carteiras de investimento
  - DTOs: `CarteiraRequest`, `CarteiraResponse`, `CarteiraComDetalhesResponse`
  - Mapper: `CarteiraMapper`
  - Service: `ICarteiraService` / `CarteiraService`
  - Endpoints `/api/v1/carteiras`:
    - GET `/` - Listar carteiras do usuário
    - GET `/{id}` - Obter carteira por ID
    - GET `/{id}/detalhes` - Obter carteira com ativos e transações
    - POST `/` - Criar carteira
    - PUT `/{id}` - Atualizar carteira
    - DELETE `/{id}` - Excluir carteira (bloqueado se houver transações)
  - Regra: Verificar ownership via `UsuarioPossuiCarteiraAsync()`

- ✅ **TransacaoService**: Gestão de transações financeiras
  - DTOs: `TransacaoRequest`, `TransacaoResponse`, `TransacaoComDetalhesResponse`
  - Constants: `TipoTransacao` (Compra, Venda, Dividendo, JCP, Bonus, Split, Grupamento)
  - Mapper: `TransacaoMapper`
  - Service: `ITransacaoService` / `TransacaoService`
  - Endpoints `/api/v1/transacoes`:
    - GET `/{id}` - Obter transação por ID
    - POST `/` - Criar transação
    - PUT `/{id}` - Atualizar transação
    - DELETE `/{id}` - Excluir transação
  - Endpoints `/api/v1/carteiras/{carteiraId}/transacoes`:
    - GET `/` - Listar transações da carteira
    - GET `/periodo?inicio=&fim=` - Filtrar por período
  - Validações:
    - ✅ Saldo suficiente para venda (calcula posição atual considerando compras/vendas/split/grupamento)
    - ✅ Preço > 0
    - ✅ Tipo de transação válido
    - ✅ Data não no futuro
    - ✅ Carteira ownership
    - ✅ Ativo existe

**Recursos Adicionais**:
- ✅ Validação de saldo para vendas com cálculo de posição
- ✅ Suporte a eventos corporativos (Split e Grupamento)
- ✅ Proteção contra exclusão de carteiras com transações (RESTRICT)
- ✅ Autorização rigorosa em todas as operações

---

### ✅ FASE 3: POSIÇÃO CONSOLIDADA (COMPLETO)

**Objetivo**: Calcular posição atual, preço médio e rentabilidade de cada ativo

**Status**: ✅ Concluído

**Implementado**:
- ✅ **DTOs de Posição**:
  - `PosicaoAtivoResponse` - Posição individual de um ativo (quantidade, preço médio, valor investido, dividendos)
  - `PosicaoConsolidadaResponse` - Posição de toda a carteira com totalizadores
  - `DistribuicaoTipoResponse` - Distribuição percentual por tipo de ativo

- ✅ **PosicaoService**: Cálculo completo de posições
  - Service: `IPosicaoService` / `PosicaoService`
  - Métodos:
    - `CalcularPosicaoAsync(carteiraId, usuarioId)` - Posição consolidada da carteira
    - `CalcularPosicaoAtivoAsync(carteiraId, ativoId, usuarioId)` - Posição de um ativo específico
    - `CalcularTodasPosicoesAsync(usuarioId)` - Todas as carteiras do usuário

- ✅ **Algoritmo WAC (Weighted Average Cost)**:
  - Compra: Recalcula preço médio ponderado
  - Venda: Reduz quantidade, mantém preço médio
  - Dividendo/JCP: Acumula proventos (não afeta preço médio)
  - Bonus: Aumenta quantidade, recalcula preço médio
  - Split: Multiplica quantidade, divide preço médio
  - Grupamento: Divide quantidade, multiplica preço médio
  - Zera preço médio quando posição é completamente vendida

- ✅ **Endpoints `/api/v1/carteiras/{carteiraId}/posicao`**:
  - GET `/{carteiraId}/posicao` - Posição consolidada da carteira
  - GET `/{carteiraId}/posicao/{ativoId}` - Posição de um ativo específico

- ✅ **Endpoint `/api/v1/posicoes`**:
  - GET `/` - Posições de todas as carteiras do usuário

- ✅ **Recursos Adicionais**:
  - Cálculo de dividendos recebidos por ativo
  - Distribuição por tipo de ativo (Ação, FII, ETF, etc.)
  - Data da primeira compra e última transação
  - Suporte completo a eventos corporativos
  - Autorização por ownership em todas as operações
  - Estrutura preparada para integração futura com APIs de cotação (PrecoAtual, ValorAtual, Lucro, Rentabilidade)

---

### ✅ FASE 4: IMPORTAÇÃO DE NOTAS DE CORRETAGEM (COMPLETO)

**Objetivo**: Importar transações automaticamente de PDFs de corretoras

**Status**: ✅ Concluído

**Implementado**:
- ✅ **Pacote NuGet**: `itext7` versão 8.0.5 para parsing de PDF

- ✅ **Value Objects** (Domain Layer):
  - `NotaCorretagem` - Representa nota completa com número, corretora, data, operações e custos
  - `OperacaoNota` - Operação individual (ticker, tipo C/V, quantidade, preços, taxas)
  - `CustosNota` - Custos agregados (liquidação, emolumentos, ISS, corretagem, outros)

- ✅ **DTOs de Importação**:
  - `ImportacaoRequest` - CarteiraId e CorretoraTipo (Clear ou XP)
  - `ImportacaoResponse` - Sucesso, contadores, erros, avisos, preview de transações

- ✅ **Strategy Pattern para Parsers PDF**:
  - `IPdfParserStrategy` - Interface base para parsers
  - `ClearPdfParser` - Parser específico para notas da corretora Clear
    - Regex para extração de número da nota, data pregão, operações e custos
    - Suporta formato Clear com tickers brasileiros (PETR4, IVVB11, etc.)
  - `XPPdfParser` - Parser específico para notas da corretora XP
    - Regex adaptado para formato XP
    - Padrões alternativos para maior compatibilidade
  - `PdfParserService` - Orquestrador que seleciona o parser correto

- ✅ **Algoritmo de Distribuição Proporcional de Custos**:
  - Distribui custos totais da nota proporcionalmente ao valor de cada operação
  - Fórmula: `CustoOperação = (ValorOperação / ValorTotalOperações) * CustoTotal`
  - Preço final ajustado: `PrecoFinal = PrecoUnitário + (CustosProporcionais / Quantidade)`

- ✅ **ImportacaoService**:
  - `PreviewImportacaoAsync()` - Visualização sem salvar no banco
  - `ImportarNotaAsync()` - Importação definitiva com salvamento
  - Auto-criação de ativos desconhecidos com tipo "Acao" (editável depois)
  - Detecção de duplicatas (verifica transações ±1 dia)
  - Verificação de ownership da carteira
  - Sistema de avisos para alertar sobre duplicatas e ativos auto-criados

- ✅ **Endpoints `/api/v1/importacao`**:
  - POST `/preview` - Preview da importação sem salvar
    - Upload multipart/form-data
    - Retorna preview completo das transações
  - POST `/confirmar` - Importar e salvar transações
    - Persiste no banco de dados
    - Retorna resumo (criadas/ignoradas)

- ✅ **Validações de Segurança**:
  - Tamanho máximo: 5MB
  - Content-type: application/pdf
  - Verificação de magic bytes (%PDF) para garantir PDF válido
  - Ownership da carteira
  - Sanitização de inputs

- ✅ **Recursos Adicionais**:
  - Parsing robusto com tratamento de erros por operação
  - Sistema de erros e avisos separados
  - Preview permite revisão antes de confirmar
  - Suporte a múltiplas operações por nota
  - Conversão automática de formato brasileiro (vírgula/ponto)

---

### 🔄 FASE 5: RELATÓRIOS E MÉTRICAS (PENDENTE)

**Objetivo**: Gerar relatórios financeiros e calcular métricas de rentabilidade

**Status**: ⏳ Não iniciado

**Pendente**:
- ⏳ Pacotes: `QuestPDF` (2024.12.3), `ClosedXML` (0.104.2)
- ⏳ Calculadoras financeiras:
  - `IrrCalculator` - Internal Rate of Return (Newton-Raphson)
  - `TwrCalculator` - Time-Weighted Return
- ⏳ DTOs: `RelatorioRentabilidadeResponse`, `RelatorioProventosResponse`, `RendimentoMensalResponse`, `ProventoAtivoResponse`
- ⏳ Service: `IRelatorioService` / `RelatorioService`
- ⏳ Endpoints `/api/v1/relatorios`:
  - GET `/rentabilidade/{carteiraId}?inicio=&fim=` - JSON
  - GET `/proventos/{carteiraId}?inicio=&fim=` - JSON
  - GET `/{carteiraId}/pdf?inicio=&fim=` - Arquivo PDF
  - GET `/{carteiraId}/excel?inicio=&fim=` - Arquivo XLSX
- ⏳ Exportação PDF com QuestPDF
- ⏳ Exportação Excel com ClosedXML

---

## Próximos Passos

**Concluído**: ✅ Fase 1 (Autenticação JWT), ✅ Fase 2 (Serviços CRUD), ✅ Fase 3 (Posição Consolidada) e ✅ Fase 4 (Importação PDF)

**Próximo**: 🎯 Fase 5 - Implementar RelatorioService (Relatórios e métricas financeiras - IRR, TWR)

**Ordem recomendada**:
1. UsuarioService (depende de AuthService para contexto de usuário)
2. CarteiraService (depende de UsuarioService para ownership)
3. TransacaoService (depende de CarteiraService para validações)
4. PosicaoService (usa TransacaoService para cálculos)
5. ImportacaoService (usa TransacaoService para salvar)
6. RelatorioService (usa PosicaoService e TransacaoService para métricas)

---

## Arquivos de Referência

**Para seguir padrões existentes, consulte**:
- `/Investment/Investment.Application/Services/AtivoService.cs` - Padrão de serviço
- `/Investment/Investment.Application/Services/AuthService.cs` - Validações e Result pattern
- `/Investment/Investment.Api/Endpoints/AtivoEndpoint.cs` - Padrão de endpoint
- `/Investment/Investment.Api/Endpoints/AuthEndpoint.cs` - Autenticação e autorização
- `/Investment/Investment.Application/Mappers/AtivoMapper.cs` - Padrão de mapper
- `/Investment/Investment.Infrastructure/Repositories/AtivoRepository.cs` - Uso de repositórios
- `/Investment/Investment.Api/Program.cs` - Configuração e DI
