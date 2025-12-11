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

### 🔄 FASE 2: SERVIÇOS CRUD (PENDENTE)

**Objetivo**: Implementar serviços completos para Usuario, Carteira e Transacao

**Status**: ⏳ Não iniciado

**Pendente**:
- ⏳ **UsuarioService**: CRUD de usuários com autorização por ownership
  - DTOs: `UsuarioRequest`, `UsuarioResponse`, `UsuarioComCarteirasResponse`
  - Mapper: `UsuarioMapper`
  - Endpoint: `/api/v1/usuarios`
  - Regra: Usuários só acessam próprios dados

- ⏳ **CarteiraService**: Gestão de carteiras de investimento
  - DTOs: `CarteiraRequest`, `CarteiraResponse`, `CarteiraComDetalhesResponse`
  - Mapper: `CarteiraMapper`
  - Endpoint: `/api/v1/carteiras`
  - Regra: Verificar ownership via `UsuarioPossuiCarteiraAsync()`

- ⏳ **TransacaoService**: Gestão de transações financeiras
  - DTOs: `TransacaoRequest`, `TransacaoResponse`, `TransacaoComDetalhesResponse`
  - Constants: `TipoTransacao` (Compra, Venda, Dividendo, JCP, Bonus, Split, Grupamento)
  - Mapper: `TransacaoMapper`
  - Endpoint: `/api/v1/transacoes` e `/api/v1/carteiras/{id}/transacoes`
  - Validações: Saldo suficiente para venda, preço > 0, carteira ownership

---

### 🔄 FASE 3: POSIÇÃO CONSOLIDADA (PENDENTE)

**Objetivo**: Calcular posição atual, preço médio e rentabilidade de cada ativo

**Status**: ⏳ Não iniciado

**Pendente**:
- ⏳ Algoritmo **Weighted Average Cost (WAC)** para cálculo de preço médio
- ⏳ Suporte a eventos corporativos: Split, Grupamento
- ⏳ Cálculo de dividendos recebidos
- ⏳ DTOs: `PosicaoAtivoResponse`, `PosicaoConsolidadaResponse`, `DistribuicaoTipoResponse`
- ⏳ Service: `IPosicaoService` / `PosicaoService`
- ⏳ Endpoint: `/api/v1/carteiras/{id}/posicao`
- ⏳ Performance: Cache com `IMemoryCache` (TTL 5min)

---

### 🔄 FASE 4: IMPORTAÇÃO DE NOTAS DE CORRETAGEM (PENDENTE)

**Objetivo**: Importar transações automaticamente de PDFs de corretoras

**Status**: ⏳ Não iniciado

**Pendente**:
- ⏳ Pacote: `itext7` (versão 8.0.5)
- ⏳ Value Objects: `NotaCorretagem`, `OperacaoNota`, `CustosNota`
- ⏳ DTOs: `ImportacaoRequest`, `ImportacaoResponse`
- ⏳ Strategy Pattern para parsers:
  - `IPdfParserStrategy` interface
  - `ClearPdfParser` - Parser para corretora Clear/XP
  - `XPPdfParser` - Parser alternativo para XP
  - `PdfParserService` - Orquestrador
- ⏳ Service: `IImportacaoService` / `ImportacaoService`
- ⏳ Endpoints `/api/v1/importacao`:
  - POST `/preview` - Preview sem salvar
  - POST `/confirmar` - Importar e salvar
- ⏳ Algoritmo de distribuição proporcional de custos
- ⏳ Auto-criação de ativos desconhecidos
- ⏳ Validações: Tamanho máx 5MB, apenas PDF, detecção de duplicatas

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

**Atual**: ✅ Fase 1 completa (Autenticação JWT)

**Próximo**: 🎯 Fase 2 - Implementar UsuarioService, CarteiraService e TransacaoService

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
