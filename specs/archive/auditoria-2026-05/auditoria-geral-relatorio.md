# Relatório de Auditoria Geral — FirstWebApi

**Data**: 2026-05-23
**Escopo**: Todo o repositório (Domain, Application, Infrastructure, WebApi, Testes, Docker, CI/CD)
**Specs auditadas**: 00 (Arquitetura), 01 (Coding Standards), 02 (Segurança), 03 (API), 04 (EF Core), 05 (Logging), 06 (Docker/CI), 07 (Testes), 08 (Anti-Patterns)

---

## Sumário Executivo

| Prioridade | Quantidade |
|------------|-----------|
| **P0** (Segurança/Falha crítica) | 1 |
| **P1** (Arquitetura/Comportamento) | 7 |
| **P2** (Estilo/Convenção) | 35 |
| **Total** | **43** |

---

## Ordem de Correção Sugerida

1. **P0** — Refresh token em JSON body (vulnerabilidade XSS)
2. **P1** — Dockerfile roda como root (`USER appuser` ausente)
3. **P1** — CI não dispara em `feature/*`/`fix/*`/`develop`
4. **P1** — security.yml não dispara em `develop`
5. **P1** — Resquícios de Redis não utilizados (connection string, serviço, CI)
6. **P1** — `PaginatedResult.Items` → `data` viola contrato da API
7. **P1** — `TotalPages` com `[JsonIgnore]` viola contrato da API
8. **P1** — AuthControllerTests não estende IntegrationTestBase
9. **P2** — CancellationToken ausente em toda a cadeia
10. **P2** — Demais itens de estilo e convenção

---

## P0 — Segurança / Falha Crítica

### P0-001: Refresh token em JSON body em vez de HttpOnly Cookie

| Campo | Valor |
|-------|-------|
| **Arquivo** | `src/FirstWebApi.WebApi/Controllers/AuthController.cs` |
| **Linhas** | 24-27, 31-34 |
| **Spec violada** | `02-security-owasp.md` — A07 (Authentication Failures) |
| **Problema** | Refresh token é retornado no body JSON (`AuthResponse.RefreshToken`) e recebido via `[FromBody] RefreshTokenRequest`. A spec determina: *"Refresh token: HttpOnly + Secure + SameSite=Strict cookie (nunca JSON body)"*. Isso expõe o refresh token a ataques XSS. |
| **Sugestão** | Mover refresh token para cookie HttpOnly+Secure+SameSite=Strict. `LoginAsync`/`RefreshTokenAsync` devem setar o cookie na resposta e não incluir `RefreshToken` no body. O endpoint `/refresh` deve ler o cookie. |

---

## P1 — Arquitetura / Comportamento

### P1-001: Dockerfile roda como root (`USER appuser` ausente)

| Campo | Valor |
|-------|-------|
| **Arquivo** | `Dockerfile` |
| **Linha** | ~42 (após `FROM ... AS runtime`) |
| **Spec violada** | `06-docker-ci.md` — "Nunca rodar como root" |
| **Problema** | Container roda como root. Se a aplicação for comprometida, atacante tem controle total do container. |
| **Sugestão** | Adicionar `USER appuser` após `WORKDIR /app` no estágio runtime. |

### P1-002: CI triggers não incluem `develop`, `feature/*`, `fix/*`

| Campo | Valor |
|-------|-------|
| **Arquivo** | `.github/workflows/ci.yml` |
| **Linhas** | 3-7 |
| **Spec violada** | `06-docker-ci.md` — "Trigger: push para develop, main, feature/*, fix/*" |
| **Problema** | CI só dispara em `main`. CI não roda em branches de feature/fix/develop. |
| **Sugestão** | Adicionar `develop`, `feature/*`, `fix/*` aos triggers de push. |

### P1-003: Security scan não dispara em `develop`

| Campo | Valor |
|-------|-------|
| **Arquivo** | `.github/workflows/security.yml` |
| **Linha** | 7 |
| **Spec violada** | `06-docker-ci.md` — "Trigger: push para develop e main" |
| **Problema** | Security scan só roda em push para `main`, não para `develop`. |
| **Sugestão** | Adicionar `develop` aos triggers. |

### P1-004: Redis connection string não utilizada

| Campo | Valor |
|-------|-------|
| **Arquivo** | `src/FirstWebApi.WebApi/appsettings.json` |
| **Linha** | 16 |
| **Spec violada** | Limpeza de configuração morta |
| **Problema** | `"Redis": "localhost:6379"` em `ConnectionStrings` mas nenhum código consome. Program.cs usa `AddMemoryCache()`, não Redis. |
| **Sugestão** | Remover connection string Redis ou implementar cache Redis. |

### P1-005: Serviço redis não utilizado no docker-compose

| Arquivo | Linhas |
|---------|--------|
| `docker-compose.yml` | 36-47, 89, 99, 111 |
| **Problema** | Container redis rodando sem propósito, consumindo recursos. |
| **Sugestão** | Remover serviço redis e volume `redis_data`, ou implementar cache Redis. |

### P1-006: Serviço redis não utilizado no CI

| Arquivo | Linhas |
|---------|--------|
| `.github/workflows/ci.yml` | 27-30, 61 |
| **Problema** | Container redis sobe no CI sem ser usado por nenhum teste/código. |
| **Sugestão** | Remover service redis e env `ConnectionStrings__Redis` do CI. |

### P1-007: `PaginatedResult.Items` viola contrato `data`

| Campo | Valor |
|-------|-------|
| **Arquivo** | `src/FirstWebApi.Application/DTOs/Response/PaginatedResult.cs` |
| **Linha** | 7 |
| **Spec violada** | `03-api-conventions.md` — Paginação |
| **Problema** | Contrato especifica `{ data: [...] }`, mas serializa como `"items"`. |
| **Sugestão** | Renomear `Items` para `Data` ou adicionar `[JsonPropertyName("data")]`. |

### P1-008: `TotalPages` com `[JsonIgnore]`

| Campo | Valor |
|-------|-------|
| **Arquivo** | `src/FirstWebApi.Application/DTOs/Response/PaginatedResult.cs` |
| **Linhas** | 12-13 |
| **Spec violada** | `03-api-conventions.md` — Paginação |
| **Problema** | `totalPages` exigido pelo contrato é excluído da serialização. |
| **Sugestão** | Remover `[JsonIgnore]` de `TotalPages`. |

### P1-009: AuthControllerTests não estende IntegrationTestBase

| Campo | Valor |
|-------|-------|
| **Arquivo** | `tests/FirstWebApi.IntegrationTests/Controllers/AuthControllerTests.cs` |
| **Linha** | 11 |
| **Spec violada** | `07-testing-strategy.md` — consistência de estrutura |
| **Problema** | Único teste de integração que não estende `IntegrationTestBase`. Duplica `_client` e `JsonContent`. |
| **Sugestão** | Refatorar para herdar `IntegrationTestBase`. |

---

## P2 — Estilo / Convenção

### Domain (3 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 1 | `src/FirstWebApi.Domain/ValueObjects/Cpf.cs` | 3 | `record class` em vez de `readonly record struct` | Mudar para `readonly record struct` |
| 2 | `src/FirstWebApi.Domain/ValueObjects/Email.cs` | 5 | `record class` em vez de `readonly record struct` | Mudar para `readonly record struct` |
| 3 | `src/FirstWebApi.Domain/Interfaces/IComicRepository.cs` | 7-11 | XML doc menciona `.Include(c => c.ComicType)` (EF Core) | Remover referência a infraestrutura |

### Application (5 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 4 | Todas as interfaces `I*Service` e implementações | Múltiplas | `CancellationToken` ausente como último parâmetro | Adicionar `CancellationToken cancellationToken = default` |
| 5 | `DTOs/Request/*.cs` | Múltiplas | Propriedades usam `{ get; set; }` em vez de `{ get; init; }` | Trocar para `init` |
| 6 | `Interfaces/IComicTypeService.cs`, `Services/ComicTypeService.cs` | 8, 18 | Parâmetro `nome` em português | Renomear para `name` |
| 7 | `Services/AuthService.cs`, `Services/ProfileService.cs` | 47, 34, 62 | Magic string `"User"` para role | Criar constante `Roles.User` |
| 8 | `Services/ComicService.cs` | 80-93 | `MapToResponse` com block body em vez de expression-bodied | Converter para `=>` |

### Infrastructure (11 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 9 | `Repositories/UserRepository.cs` | 13, 18 | `FirstOrDefaultAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 10 | `Repositories/ComicRepository.cs` | 31-33 | `GetByIdAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 11 | `Repositories/ComicTypeRepository.cs` | 13-15 | `ToListAsync()` sem paginação | Documentar ou adicionar paginação |
| 12 | `Repositories/ComicTypeRepository.cs` | 13-15 | `GetAllAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 13 | `Repositories/AddressRepository.cs` | 13 | `FirstOrDefaultAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 14 | `Repositories/RefreshTokenRepository.cs` | 13-14 | `GetByTokenHashAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 15 | `Repositories/RefreshTokenRepository.cs` | 19-21 | `GetActiveByUserIdAsync` sem `AsNoTracking()` | Adicionar `.AsNoTracking()` |
| 16 | `Services/TokenService.cs` | 23-25 | JWT SecretKey sem validação de tamanho mínimo (≥32 chars) | Adicionar validação |
| 17 | `Logging/FileLogger.cs` | 63 | Mensagem formatada não passa por redação de dados sensíveis | Aplicar redação no texto da mensagem |
| 18 | `Logging/FileLogger.cs` | 118 | `File.AppendAllText` síncrono em pipeline de log | Usar `File.AppendAllTextAsync` ou ignorar (ILogger.Log é síncrono) |

### WebApi (7 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 19 | `Controllers/AuthController.cs` | 10-14 | Sem `[Authorize]` no controller (apenas `[AllowAnonymous]` nos endpoints públicos) | Adicionar `[Authorize]` na classe |
| 20 | `Controllers/AuthController.cs` | 17, 23, 30, 37 | Nenhum endpoint tem `[ProducesResponseType]` | Adicionar em todos os endpoints |
| 21 | `Controllers/UsersController.cs` | 19, 34 | `GetProfile` e `GetFullProfile` sem `[ProducesResponseType]` | Adicionar |
| 22 | Todos os controllers | Múltiplas | Nenhum passa `CancellationToken` para os services | Propagar `HttpContext.RequestAborted` |
| 23 | `Controllers/UsersController.cs` | 30 | `MaskingHelper.ApplyMask` chamado diretamente no controller (lógica de negócio) | Mover para Application Service |
| 24 | `Controllers/AuthController.cs` | 20 | `CreatedAtAction(nameof(Register), ...)` gera Location inválido | Apontar para recurso real (`UsersController.GetProfile`) |
| 25 | `Controllers/AdminComicTypesController.cs` | 26 | `CreatedAtAction(null, ...)` gera Location sem ação | Usar `nameof(GetById)` ou `Created` |

### Docker/CI (5 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 26 | `docker-compose.yml` | Todos | Healthchecks ausentes em sqlserver, redis, localstack, webapi | Adicionar healthcheck em cada serviço |
| 27 | `.github/workflows/ci.yml` | — | Vulnerability scan (`dotnet list package --vulnerable`) ausente no CI | Adicionar step |
| 28 | `.github/workflows/ci.yml` | 55, 58 | Cobertura de código coletada mas não publicada | Adicionar `actions/upload-artifact` |
| 29 | `docker-compose.yml` | 38 | Comentário obsoleto "StackExchange.Redis" | Atualizar ou remover |
| 30 | `docker-compose.yml` | 111 | Volume `redis_data` não utilizado (se Redis removido) | Remover junto com serviço |
| 31 | `.github/workflows/ci.yml` | 54 | Step name "Testes Unitários" com acento e português | Renomear para "Unit Tests" |
| 32 | `.github/workflows/security.yml` | — | Nenhum comment sobre propósito do workflow | Adicionar comentário descritivo |

### Testes (7 ocorrências)

| # | Arquivo | Linha | Problema | Sugestão |
|---|---------|-------|----------|----------|
| 33 | `tests/FirstWebApi.UnitTests/Domain/UserTests.cs` | 82 | Naming `CreateUser_ShouldHaveValidId` sem estado | Renomear para `CreateUser_WithValidData_ShouldHaveValidId` |
| 34 | `tests/FirstWebApi.UnitTests/Infrastructure/TokenServiceTests.cs` | 56-57 | Linha em branco duplicada antes do EOF | Remover linha extra |
| 35 | `tests/FirstWebApi.IntegrationTests/DatabaseCollection.cs` | 5-8 | `ICollectionFixture<FirstWebApiFactory>` nunca consumido | Remover ou reestruturar |
| 36 | `tests/FirstWebApi.IntegrationTests/IntegrationTestBase.cs` | 93-97 | Código morto: `SetAuthHeader` nunca chamado | Remover ou refatorar callers |
| 37 | `tests/FirstWebApi.IntegrationTests/Controllers/AuthControllerTests.cs` | 20-21 | `JsonContent` duplicado (já existe em IntegrationTestBase) | Herdar IntegrationTestBase resolve |
| 38 | `tests/FirstWebApi.IntegrationTests/Controllers/AuthControllerTests.cs` | 128-200 | 4x `[Fact]` para 400 scenarios (mesmo comportamento) | Consolidar em `[Theory]` com `[InlineData]` |
| 39 | Geral — testes de integração | — | Cobertura de 422 (erro de negócio) inexistente | Adicionar testes para 422 |

---

## Detalhamento por Camada

### Domain — 12 arquivos auditados, 3 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | `FirstWebApi.Domain.csproj`, `Entities/Address.cs`, `Entities/Comic.cs`, `Entities/ComicType.cs`, `Entities/RefreshToken.cs`, `Entities/User.cs`, `ValueObjects/DadoProtegido.cs`, `Interfaces/IAddressRepository.cs`, `Interfaces/IComicTypeRepository.cs`, `Interfaces/IRefreshTokenRepository.cs`, `Interfaces/IUnitOfWork.cs`, `Interfaces/IUserRepository.cs` |
| ⚠️ Violações | `ValueObjects/Cpf.cs`, `ValueObjects/Email.cs`, `Interfaces/IComicRepository.cs` |

### Application — 33 arquivos auditados, 5 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | Todos os DTOs/Response, Exceptions, Validators, DTOs/EnderecoInfo.cs |
| ⚠️ Violações | Todas as interfaces de serviço (7), Services/ComicService.cs, Services/ComicTypeService.cs, Services/AuthService.cs, Services/ProfileService.cs, Services/SensitiveDataService.cs |

### Infrastructure — 20 arquivos auditados, 11 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | `Data/AppDbContext.cs`, todas as Configurations (5), `KmsEncryptionService.cs`, `RefreshTokenCleanupService.cs`, `EncryptedData.cs`, `.csproj`, Migrations |
| ⚠️ Violações | Todos os Repositories (5), `TokenService.cs`, `FileLogger.cs` |

### WebApi — 12 arquivos auditados, 10 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | `Middleware/ExceptionMiddleware.cs`, `Middleware/SecurityHeadersMiddleware.cs`, `Program.cs`, `Extensions/ClaimsPrincipalExtensions.cs`, `Helpers/MaskingHelper.cs`, `DatabaseInitializer.cs`, `ComicsController.cs`, `ComicTypesController.cs`, `AdminComicTypesController.cs` (com ressalvas) |
| ⚠️ Violações | `Controllers/AuthController.cs`, `Controllers/UsersController.cs`, `DTOs/Response/PaginatedResult.cs` |

### Docker/CI/Redis — 5 arquivos auditados, 14 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | `.dockerignore`, `coverlet.runsettings` |
| ⚠️ Violações | `Dockerfile`, `docker-compose.yml`, `.github/workflows/ci.yml`, `.github/workflows/security.yml` |

### Testes — 15 arquivos auditados, 7 violações

| Status | Arquivos |
|--------|----------|
| ✅ Compliant | `AuthServiceTests.cs`, `ComicServiceTests.cs`, `ComicTypeServiceTests.cs`, `ProfileServiceTests.cs`, `KmsEncryptionServiceTests.cs`, `FirstWebApiFactory.cs`, `UserControllerTests.cs`, `ComicTypesControllerTests.cs`, `ComicsControllerTests.cs`, ambos `.csproj` |
| ⚠️ Violações | `UserTests.cs`, `TokenServiceTests.cs`, `DatabaseCollection.cs`, `IntegrationTestBase.cs`, `AuthControllerTests.cs` |

---

## Checklist de Auditoria

- [x] Domain verificado contra specs 00, 01, 08 — **3 violações P2**
- [x] Application verificado contra specs 00, 01, 03, 08 — **1 P1, 4 P2**
- [x] Infrastructure verificado contra specs 02, 04, 05, 08 — **11 P2**
- [x] WebApi verificado contra specs 00, 01, 03, 08 — **1 P0, 2 P1, 7 P2**
- [x] Docker/CI verificado contra spec 06 — **3 P1, 5 P2**
- [x] Redis remnants identificados — **3 P1, 2 P2**
- [x] Testes verificados contra spec 07 — **1 P1, 6 P2**
- [x] Relatório priorizado entregue
- [x] Ordem de correção sugerida

---

## Estatísticas Finais

| Camada | P0 | P1 | P2 | Total |
|--------|----|----|----|-------|
| Domain | 0 | 0 | 3 | 3 |
| Application | 0 | 1 | 4 | 5 |
| Infrastructure | 0 | 0 | 11 | 11 |
| WebApi | 1 | 2 | 7 | 10 |
| Docker/CI/Redis | 0 | 3 | 5 | 8 |
| Tests | 0 | 1 | 6 | 7 |
| **Total** | **1** | **7** | **35** | **43** |
