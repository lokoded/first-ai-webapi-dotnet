# AGENTS.md — FirstWebApi

> Contexto persistente do projeto. A IA carrega este arquivo para entender
> o estado atual, decisões passadas e convenções do projeto.
> Mantenha atualizado como "diário de bordo" do desenvolvimento.

Web API .NET 10 em Clean Architecture para gerenciamento de coleção de HQs com criptografia LGPD (KMS), autenticação JWT, cache Redis e SQL Server.

## Stack Resumida (para consulta rápida da IA)

```
.NET 10 | EF Core + SQL Server | Redis | JWT | KMS | FluentValidation | xUnit + Moq
```

## Status Atual do Projeto

- Build: ✅ 0 erros, 0 warnings
- Testes unitários: ✅ 35/35
- Testes de integração: ✅ 21/21 (Docker Compose rodando)
- GitHub Actions: ✅ CI + Security Scan
- Módulo atual: 3 (CI/CD)

## Comandos

```powershell
# Compilar
dotnet build

# Rodar API localmente (porta 5043 via launchSettings)
dotnet run --project src/FirstWebApi.WebApi

# Testes unitários (sem infra necessária)
dotnet test tests/FirstWebApi.UnitTests

# Testes de integração (requer docker compose up -d primeiro)
dotnet test tests/FirstWebApi.IntegrationTests

# Todos os testes
dotnet test

# Migrations do EF
dotnet ef migrations add <Name> --project src/FirstWebApi.Infrastructure --startup-project src/FirstWebApi.WebApi

# Docker
docker compose up -d                      # inicia SQL Server + Redis + LocalStack + API
docker compose down                       # para
docker compose down -v                    # para + apaga TODOS os dados (volumes)

# Script idempotente de migration (para produção)
dotnet ef migrations script --idempotent -o scripts/migration.sql

# Script de rollback entre duas versões
dotnet ef migrations script <De> <Para> --idempotent -o scripts/rollback.sql
```

## Arquitetura

```
FirstWebApi.slnx
├── src/FirstWebApi.Domain/           # Entities, ValueObjects, Enums, Repository interfaces
├── src/FirstWebApi.Application/      # DTOs, Service interfaces/impl, FluentValidation validators
├── src/FirstWebApi.Infrastructure/   # EF Core DbContext, Repositories, Migrations, KMS, JWT, Redis cache decorators
└── src/FirstWebApi.WebApi/           # Controllers, Middleware, Program.cs (entrypoint)
```

- Solução usa `.slnx` (novo formato XML), não `.sln`
- Domain não tem dependências NuGet (exceto `Microsoft.Extensions.Identity.Stores` para `IdentityUser<Guid>`)
- WebApi referencia apenas Application + Infrastructure; IntegrationTests referencia WebApi (com `InternalsVisibleTo`)

## API Endpoints

| Método | Rota | Auth | Role | Descrição |
|--------|------|------|------|-----------|
| POST | `/api/auth/register` | ❌ | — | Cadastro de usuário |
| POST | `/api/auth/login` | ❌ | — | Login |
| GET | `/api/comics` | ✅ | — | Listar HQs do usuário |
| POST | `/api/comics` | ✅ | — | Criar HQ |
| GET | `/api/comics/{id}` | ✅ | — | Detalhe da HQ |
| PUT | `/api/comics/{id}` | ✅ | — | Atualizar HQ |
| DELETE | `/api/comics/{id}` | ✅ | — | Remover HQ |
| GET | `/api/comic-types` | ✅ | — | Listar tipos de HQ |
| POST | `/api/admin/comic-types` | ✅ | Admin | Criar tipo de HQ |
| DELETE | `/api/admin/comic-types/{id}` | ✅ | Admin | Remover tipo de HQ |
| GET | `/api/users/me` | ✅ | — | Perfil do usuário logado |
| GET | `/health` | ❌ | — | Health check |

## Peculiaridades e Armadilhas

- **`SuppressModelStateInvalidFilter = true`** em `Program.cs:23` — a validação automática do `[ApiController]` está DESLIGADA. Toda validação é manual via FluentValidation nos controllers.
- **Migration automática na inicialização**: `Program.cs` executa `dbContext.Database.Migrate()` **apenas** em ambiente `Development` ou `Testing`. Em produção, gerar script idempotente (`dotnet ef migrations script --idempotent`) e revisar via PR antes de aplicar.
- **Auto-seed de roles**: `Program.cs:125-128` cria as roles "User" e "Admin" via `RoleManager` na inicialização.
- **KMS encryption** (`KmsEncryptionService`) chama `InitializeKeyAsync().GetAwaiter().GetResult()` no construtor. Vai travar se o LocalStack não estiver rodando. O serviço cria a chave KMS automaticamente se não existir.
- **Redis caching** via decorators do Scrutor (`CachedComicRepository`, etc.) — registrados em `Program.cs:83-86`. Padrão cache-aside.
- **Testes de integração** usam `WebApplicationFactory<Program>` com `builder.UseSetting("Environment", "Testing")` para carregar `appsettings.Testing.json`. Todos os testes que dependem de banco estão em `[Collection("Database")]` (sequenciais, banco compartilhado `FirstWebApiDb_Test`).
- **`.env` está no .gitignore** — criar a partir do template em `.env` (contém `SA_PASSWORD`, `CONNECTION_STRING`, `LOCALSTACK_AUTH_TOKEN`). LocalStack v4+ exige um auth token gratuito.
- **Português brasileiro** nas strings voltadas ao usuário (mensagens de erro, validação), inglês nos identificadores do código.
- **Health endpoint**: `GET /health` retorna `{ status: "healthy", timestamp }` (usado pelo Docker HEALTHCHECK).
- **ValueObjects no Domain**: `Cpf` e `Email` são structs com validação própria. Nunca armazenar CPF/email como `string` pura no domínio.
- **LGPD / KMS envelope encryption**: CPF e RG do `User` são cifrados via `IEncryptionService` (AWS KMS `GenerateDataKey` + AES-256). Dados ficam em `byte[]` (`CpfCiphertext`, `CpfIv`, `CpfTag`, `CpfEncryptedDataKey`). Sempre usar o serviço de criptografia, nunca armazenar texto puro.
- **Role padrão**: todo usuário registrado recebe "User". Admin é promovido manualmente no banco.
- **JSON é o formato padrão**: controllers **não** usam `[Produces("application/json")]` — o ASP.NET Core já negocia JSON como default.

## Como adicionar uma nova entidade (CRUD)

1. **Domain**: Criar entity + interface de repositório
2. **Application**: DTOs (Request/Response) + FluentValidation Validator + Service interface/impl
3. **Infrastructure**: EF Core Repository + `IEntityTypeConfiguration` + `DbSet` no `AppDbContext`
4. **WebApi**: Registrar DI em `Program.cs` + Controller usando `Problem()`/`ValidationProblem()`
5. **Testes**: Unit tests (Moq) + Integration tests (`WebApplicationFactory`)

## RFC 9457 — Problem Details (HTTP API Errors)

Todas as respostas de erro usam `application/problem+json` conforme RFC 9457.

- **Unhandled exceptions** → `ExceptionMiddleware` (src/FirstWebApi.WebApi/Middleware/ExceptionMiddleware.cs) serializa `ProblemDetails` com `type`, `title`, `status`, `detail`, `instance`, `traceId`.
- **Erros conhecidos** (validação, conflito, não encontrado) → `ControllerBase.Problem()` ou `ValidationProblem()` nos controllers.
- **Regra**: nunca retornar `BadRequest()`, `NotFound()` ou `Conflict()` diretamente; sempre usar `Problem()` ou `ValidationProblem()` para manter o formato padronizado.
- O campo `type` deve ser uma URI documentando o problema (`about:blank` é aceitável para casos sem semântica adicional).

## Convenções de Teste

- **Unit tests**: xUnit + Moq + FluentAssertions. Pure mock-based, test service logic.
- **Integration tests**: xUnit + `WebApplicationFactory<Program>` + FluentAssertions. Seeding feito inline via `AppDbContext` do `Services` da factory. Requerem `docker compose up -d` para rodar.
- Integration tests obtêm JWT registrando um usuário primeiro via `/api/auth/register`.
- Integration tests semeiam entidades no banco diretamente via `_factory.Services.CreateScope()` → `AppDbContext`.

---

## Architecture Decision Records (ADR)

Registro de decisões arquiteturais importantes. Novas decisões devem ser adicionadas aqui com data e justificativa.

### ADR-001: Clean Architecture sem MediatR

- **Data**: 20/05/2026
- **Contexto**: Projeto de porte pequeno/médio, time unipessoal
- **Decisão**: Não usar MediatR ou CQRS. A complexidade adicional não se justifica para o escopo atual
- **Consequências**: Menos arquivos, menos indireção, mais fácil de debugar. Se o projeto crescer, pode-se adotar MediatR seletivamente
- **Revisitar quando**: Projeto atingir >20 endpoints ou time >3 devs

### ADR-002: `.slnx` em vez de `.sln`

- **Data**: 20/05/2026
- **Contexto**: .NET 10 introduziu suporte completo a `.slnx` (XML)
- **Decisão**: Usar `.slnx` por ser mais legível, versionável e fácil de diff
- **Consequências**: Ferramentas antigas podem não suportar, mas .NET 10+ e VS 2025+ têm suporte nativo

### ADR-003: FluentValidation manual (sem filtro automático)

- **Data**: 20/05/2026
- **Contexto**: `SuppressModelStateInvalidFilter = true`
- **Decisão**: Desligar validação automática do `[ApiController]` e validar manualmente
- **Consequências**: Mais código boilerplate, mas controle total sobre formato da resposta de erro e mensagens em português
- **Alternativa rejeitada**: Filtro custom de validação — adicionaria complexidade desnecessária

### ADR-004: KMS envelope encryption em vez de AWS SDK encryption helpers

- **Data**: 20/05/2026
- **Contexto**: Necessidade de criptografia LGPD para dados pessoais
- **Decisão**: Implementar envelope encryption manual (GenerateDataKey + AES-256) em vez de usar `Amazon.EncryptionSDK`
- **Consequências**: Mais controle sobre o formato dos dados cifrados, sem dependência extra. Código ligeiramente mais verboso
- **Risco**: Implementação criptográfica manual pode ter falhas sutis — revisar com especialista antes de produção

### ADR-005: Refresh Tokens com rotação

- **Data**: 20/05/2026
- **Contexto**: JWT com 8h de expiração — se o token vazar, a janela de ataque é grande
- **Decisão**: Implementar refresh tokens armazenados em banco (hash SHA256), com rotação (cada refresh gera um novo par) e revogação por usuário
- **Consequências**: Mais segurança, mas mais chamadas ao banco. Refresh tokens têm 7 dias de validade
- **Por que não JWT para refresh?**: Refresh tokens não precisam ser stateless — armazenar no DB permite revogação individual

### ADR-006: Rate Limiting nativo do .NET 10

- **Data**: 20/05/2026
- **Contexto**: Endpoints de auth sem proteção contra brute force (OWASP A07)
- **Decisão**: Usar `System.Threading.RateLimiting` (nativo) em vez de `AspNetCoreRateLimit` (biblioteca externa)
- **Consequências**: Zero dependência extra, API nativa, policies por endpoint (Auth: 10 req/min, Default: 100 req/min)
- **Alternativa rejeitada**: `AspNetCoreRateLimit` — dependência externa desnecessária

## Diário de Aprendizado

Registro de aprendizados, descobertas e lições durante o desenvolvimento.

### 20/05/2026 — Módulo 0 concluído

**Setup & Diagnóstico**
- Build 0 erros ✅, Unit tests 35/35 ✅, Integration tests 21/21 ✅
- Docker Compose com 4 containers rodando
- `.editorconfig` + `.vscode/` criados para padronização
- `.opencode/context.md` criado como baseline

**Descobertas**:
- Integration tests compartilham banco `FirstWebApiDb_Test` — rodar sempre em sequência
- KMS `GetAwaiter().GetResult()` no construtor trava se LocalStack estiver offline
- OpenCode tem agents nativos `build`, `explore`, `plan`, `general` — todos configuráveis

### 20/05/2026 — Módulo 1 concluído

**OpenCode, Agents & CLI**
- `opencode.jsonc` criado com schema oficial + agents + instructions
- `rules.md` com 45+ regras do projeto carregadas no contexto
- 3 skills no formato oficial `SKILL.md` (criar-endpoint, criar-entidade, code-review)
- 3 agents em `.opencode/agents/` (dev-agent, tester, security-reviewer)
- `.opencode/prompts.md` com templates e boas práticas
- Evolução do AGENTS.md com ADR e diário de aprendizado

**Descobertas**:
- OpenCode lê skills de `.opencode/skills/<nome>/SKILL.md` automaticamente
- Agentes em markdown com YAML frontmatter
- JSONC precisa de aspas nas chaves (`"key"`, não `key:`)

### 20/05/2026 — Módulo 2 concluído

**DevSecOps (OWASP Top 10)**
- Rate limiting nativo (.NET 10) — Auth: 10 req/min, Default: 100 req/min
- Refresh tokens com rotação (SHA256 no DB, 7 dias de validade)
- Security headers: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy
- Account lockout: 5 tentativas, 15 min de bloqueio
- CORS restrito por ambiente (via configuração)
- appsettings.json limpo (sem credenciais hardcoded)
- Migration `AddRefreshTokens` gerada
- 56/56 testes passando

**Descobertas**:
- `opencode.json` usa `"agent"` (singular) não `"agents"`
- Skills requerem SKILL.md com YAML frontmatter em subfolder
- EF Core 10 bloqueia `PendingModelChangesWarning` por padrão — precisa configurar em Dev/Testing

### 21/05/2026 — Módulo 3 concluído

**CI/CD com GitHub Actions**
- Repositório Git inicializado e enviado para GitHub (SSH)
- Pipeline CI com build + testes unitários + testes de integração
- SQL Server, Redis e LocalStack como service containers no CI
- Credenciais movidas para GitHub Secrets (segurança)
- Quality gate: cobertura mínima de 70% via coverlet + runsettings
- Badge de status do CI no README
- Workflow de varredura de vulnerabilidades (semanal + push)
- Branch `develop` criada como padrão de trabalho

**Descobertas**:
- `dotnet add package` resolve versão mais recente automaticamente (coverlet 6.0.4 → 10.0.1)
- SSH na porta 22 pode ser bloqueado por rede; alternativa é porta 443
- GitHub API retorna 404 para repositórios privados sem autenticação
- Service containers no GitHub Actions substituem o docker-compose local
