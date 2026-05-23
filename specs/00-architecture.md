# 00 — Arquitetura

## Stack

.NET 10 | ASP.NET Core | EF Core + SQL Server | Redis | JWT | KMS | FluentValidation | xUnit + Moq

## Clean Architecture — 4 Camadas

```
Domain → Application → Infrastructure → WebApi
```

Regras de dependência:
- **Domain**: zero dependências externas (exceção: `Microsoft.Extensions.Identity.Stores` para `IdentityUser<Guid>`)
- **Application**: depende apenas do Domain
- **Infrastructure**: depende da Application + Domain
- **WebApi**: depende da Application + Infrastructure (entry point)

### Domain (`src/FirstWebApi.Domain/`)
- Entities, ValueObjects, Enums
- Repository interfaces (apenas contratos)
- `IUnitOfWork` com `Task<int> SaveChangesAsync()`
- Nenhuma lógica de infraestrutura

### Application (`src/FirstWebApi.Application/`)
- DTOs (`Request/`, `Response/`)
- Services (regras de negócio + orquestração)
- Validators (FluentValidation)
- Interfaces de serviço (`I*Service`)
- Nunca expor entidades do Domain nos controllers

### Infrastructure (`src/FirstWebApi.Infrastructure/`)
- EF Core: `AppDbContext` + Configurations + Migrations
- Repositories (implementam interfaces do Domain)
- JWT, KMS, Redis decorators
- Services de infraestrutura

### WebApi (`src/FirstWebApi.WebApi/`)
- Controllers (orquestração apenas — sem lógica de negócio)
- Middleware (Exception, Security Headers, Rate Limiting)
- `Program.cs` (DI, pipeline, configurações)
- NUNCA: DbContext ou repositories nos controllers

## Monolito Modular

Arquitetura monolítica modular simples. Sem microservices, sem CQRS completo, sem event sourcing, sem kubernetes.

## RFC 9457 — Problem Details

Todas as respostas de erro usam `application/problem+json`:
- Exceções não tratadas → middleware global serializa `ProblemDetails`
- Erros conhecidos (validação, conflito, não encontrado) → `Problem()` / `ValidationProblem()` do `ControllerBase`
- NUNCA retornar `BadRequest()`, `NotFound()` ou `Conflict()` diretamente
- `ProblemDetails`: type (RFC URL), title, status, detail, instance, traceId
- 500: nunca expor detalhes internos ou stack trace

## ADRs

### ADR-001: Clean Architecture sem MediatR
Projeto de pequeno/médio porte, time unipessoal. MediatR adiciona indireção sem ganho justificável.

### ADR-002: `.slnx` em vez de `.sln`
Mais legível, versionável, fácil de diff. .NET 10+ tem suporte nativo.

### ADR-003: Auto-validation do `[ApiController]`
Validação automática do ASP.NET via FluentValidation. `SuppressModelStateInvalidFilter` NÃO está mais ativo.

### ADR-004: KMS envelope encryption
GenerateDataKey + AES-256 em vez de AWS Encryption SDK. Mais controle, zero dependência extra.

### ADR-005: Refresh tokens com rotação
Hash SHA256 no banco, rotação a cada refresh, revogação por usuário. 7 dias de validade.

### ADR-006: Rate Limiting nativo do .NET
`System.Threading.RateLimiting` em vez de `AspNetCoreRateLimit`. Auth: 10 req/min, Default: 100 req/min.

### ADR-007: DadoProtegido — blob único opaco
Domain não conhece algoritmo de criptografia. Trocar algoritmo = só alterar Infrastructure.
