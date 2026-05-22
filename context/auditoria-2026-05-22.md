# Auditoria Geral — FirstWebApi

**Data:** 2026-05-22
**Branch base:** `develop`
**Contexto:** Auditoria completa de código, arquitetura, segurança e testes.

---

## Sumário Executivo

| Categoria | 🔴 Alta | 🟡 Média | 🟢 Baixa | Total |
|-----------|---------|----------|----------|-------|
| Código em local errado | 0 | 0 | 1 | 1 |
| Código não utilizado | 0 | 2 | 4 | 6 |
| Código feito errado | 1 | 3 | 5 | 9 |
| Lógica grande e exagerada | 2 | 4 | 2 | 8 |
| Redundância | 3 | 4 | 4 | 11 |
| Código defasado | 1 | 8 | 7 | 16 |
| Soluções exageradas | 2 | 4 | 0 | 6 |
| **Total** | **9** | **25** | **23** | **57** |

---

## Itens Corrigidos (4 Fases)

### Fase 1 — Segurança ✅ (PR #15, 5 commits)
1. 🔑 JWT SecretKey via `.env` (DotNetEnv) — removido hardcoded do `appsettings.Development.json`
2. 📦 FluentValidation version mismatch corrigido (11.11.0 → 12.1.1)
3. 🛡️ FileLogger com sanitização de campos sensíveis (CPF, RG, senha, token)
4. 🚦 Rate limiting Strict (5 req/min) para endpoint `/api/users/me/full`
5. 🗑️ Removido `CachedUserRepository` e `CachedAddressRepository` (decorators sem cache real)

### Fase 2 — Refatoração AuthService ✅ (PR #16, 4 commits)
6. 📐 Constantes `RefreshTokenExpiry`, mensagens de erro e `BuildAuthResponse()` factory method
7. ♻️ `DecryptFieldAsync()` genérico — eliminados 3 blocos try/catch idênticos
8. 🧹 Validação de dígito CPF duplicada removida do validator (`Cpf` VO já valida)
9. 🆕 `ProfileService` extraído do `AuthService` (AuthService: 344→179 linhas)

### Fase 3 — Cleanup Arquitetural ✅ (PR #17, 1 commit)
10. 🏗️ 13 classes convertidas para primary constructors C# 12
11. 🔧 `TokenService` migrado de `IConfiguration` para `IOptions<JwtSettings>`
12. 📦 POCO `JwtSettings` registrado via `Configure` no `Program.cs`

### Fase 4 — Testes ✅ (PR #18, 1 commit)
13. ✅ Testes de integração adicionados: `GET /api/comics`, `GET /api/comics/{id}`, `PUT /api/comics/{id}`, `DELETE /api/comics/{id}`, `POST /api/users/me/full`
14. ✅ Testes unitários do `KmsEncryptionService` (EncryptAsync + DecryptAsync)
15. ♻️ `KmsEncryptionService` refatorado para aceitar `IAmazonKeyManagementService` via DI

---

## Itens Não Abordados (pendentes para próximas iterações)

### 🔴 Alta Prioridade Restante

| # | Problema | Arquivo | Motivo |
|---|----------|---------|--------|
| 1 | `IUnitOfWork` — abstração desnecessária | `src/.../Domain/Interfaces/IUnitOfWork.cs` | Requer refatoração arquitetural profunda (mudança na injeção de dependências entre camadas). Solução: remover interface e injetar SaveChangesAsync via repositórios ou mover para Application layer. |
| 2 | 14 interfaces para ~5500 linhas — over-abstraction | Múltiplos arquivos | Remover interfaces single-impl quebram testabilidade com mocks. Solução: avaliar se todas as interfaces são necessárias ou consolidar algumas. |

### 🟡 Média Prioridade Restante

| # | Problema | Arquivo |
|---|----------|---------|
| 3 | `IComicRepository.GetByUserIdAsync` — nunca usado | `src/.../Domain/Interfaces/IComicRepository.cs:7` |
| 4 | `IUserRepository.UpdateAsync` — nunca usado | `src/.../Domain/Interfaces/IUserRepository.cs:10` |
| 5 | `ExceptionMiddleware` mapeia `InvalidOperationException`→409 (genérico) | `src/.../Middleware/ExceptionMiddleware.cs:32` |
| 6 | `AdminComicTypesController.Create` — Location header errado | `src/.../Controllers/AdminComicTypesController.cs:34` |
| 7 | `CachedPaginatedResult` — inner class sobre-engenharia | `src/.../Decorators/CachedComicRepository.cs:87-101` |
| 8 | `KmsEncryptionService.InitializeKeyAsync` — criação de chave em runtime | `src/.../Services/KmsEncryptionService.cs:59` |
| 9 | 14 repositórios/decorators para 5 entidades — overhead | Vários arquivos |
| 10 | `RefreshToken.IsActive` property não usado | `src/.../Domain/Entities/RefreshToken.cs:27` |
| 11 | `ComicTypesController` — primary constructor inconsistente | `src/.../Controllers/ComicTypesController.cs:13-20` |
| 12 | `Database.Migrate()` síncrono | `src/.../DatabaseInitializer.cs:16` |
| 13 | Validators sem testes unitários dedicados (7 validators) | `src/.../Validators/` |
| 14 | Testes de edge case no ComicService (clamping, ComicTypeId inválido) | `tests/.../ComicServiceTests.cs` |
| 15 | Testes de `ExceptionMiddleware` e `SecurityHeadersMiddleware` | `src/.../Middleware/` |

### 🟢 Baixa Prioridade

| # | Problema |
|---|----------|
| 16 | `Email` VO nunca usado em produção |
| 17 | `ValidateToken` só usado em testes |
| 18 | `[FromBody]`/`[FromQuery]` redundantes com `[ApiController]` |
| 19 | Swashbuckle + Scalar — duplicidade de ferramentas |
| 20 | `Context/security.md` desatualizado (diz AES-CBC, código usa AES-GCM) |
| 21 | `EnderecoInfo.IsEmpty` inclui `Complemento` na checagem |
| 22 | Boilerplate de validação repetido em todos os controllers |
| 23 | Mapping manual (DTOs) pode beneficiar de AutoMapper no futuro |

---

## Decisões Arquiteturais Registradas

### ADR-007: Primary Constructors C# 12
**Decisão:** Todas as classes com construtores de simples atribuição foram convertidas para primary constructors.

**Exceções:**
- `KmsEncryptionService`: inicialização complexa com `Lazy<Task>` — convertido com adaptações (optional parameter para `IAmazonKeyManagementService`)
- `FileLoggerProvider`: lógica de criação de diretório no construtor
- `AppDbContext`: construtor `: base(options)` sem campos próprios

### ADR-008: ProfileService
**Decisão:** Extraído `ProfileService` de `AuthService` para separar responsabilidades de autenticação (register/login/refresh) de perfil de usuário (get profile, decrypt data, masking).

### ADR-009: KmsEncryptionService DI
**Decisão:** `IAmazonKeyManagementService` agora é opcional via DI. Se não fornecido, o serviço cria o cliente internamente. Permite mocking em testes.

---

## Métricas Pós-Auditoria

| Métrica | Antes | Depois |
|---------|-------|--------|
| AuthService (linhas) | 344 | 179 |
| ProfileService (novo) | — | 141 |
| Arquivos removidos | — | 2 (CachedUserRepo, CachedAddressRepo) |
| Primary constructors | ~4 | ~20 |
| IOptions vs IConfiguration | 0 | 1 (TokenService) |
| Testes de integração | ~20 | 32 |
| Testes unitários do KMS | 0 | 1 arquivo, 2 testes |

---

## Verificação Pós-Implementação (2026-05-22)

Uma segunda auditoria foi realizada **após** todas as correções para verificar regressões e precisão do relatório.

### Nenhuma Regressão Detectada

| Padrão | Status |
|--------|--------|
| `BadRequest()`/`NotFound()`/`Conflict()` nos controllers | ✅ 0 ocorrências |
| `ValidateAsync` em toda action com body | ✅ 7/7 actions validam |
| `.Result`/`.Wait()`/`.GetAwaiter()` | ✅ 0 ocorrências |
| `SuppressModelStateInvalidFilter = true` | ✅ Preservado |
| Rate limiting (Auth + Default + Strict) | ✅ Preservado |
| CORS configurado | ✅ Preservado |
| Build (0 erros, 0 warnings) | ✅ Confirmado |

### Correções Aplicadas ao Relatório

| Correção | Antes | Depois |
|----------|-------|--------|
| AuthService (linhas) | 192 | 179 |
| ProfileService (linhas) | 155 | 141 |
| Arquivos removidos | 3 (incluía IUnitOfWork pendente) | 2 |
| Primary constructors | ~17 | ~20 |
| Testes de integração | ~28 | 32 |
| Item #10 pendente (falso positivo) | `ComicTypeServiceTests` precisava atualização | Removido (compila normalmente) |
