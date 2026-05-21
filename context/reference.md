# Referência Rápida

> Fonte: AGENTS.md (extraído em 21/05/2026)

---

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

---

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

---

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

---

## Como adicionar uma nova entidade (CRUD)

1. **Domain**: Criar entity + interface de repositório
2. **Application**: DTOs (Request/Response) + FluentValidation Validator + Service interface/impl
3. **Infrastructure**: EF Core Repository + `IEntityTypeConfiguration` + `DbSet` no `AppDbContext`
4. **WebApi**: Registrar DI em `Program.cs` + Controller usando `Problem()`/`ValidationProblem()`
5. **Testes**: Unit tests (Moq) + Integration tests (`WebApplicationFactory`)

---

## Convenções de Teste

- **Unit tests**: xUnit + Moq + FluentAssertions. Pure mock-based, test service logic.
- **Integration tests**: xUnit + `WebApplicationFactory<Program>` + FluentAssertions. Seeding feito inline via `AppDbContext` do `Services` da factory. Requerem `docker compose up -d` para rodar.
- Integration tests obtêm JWT registrando um usuário primeiro via `/api/auth/register`.
- Integration tests semeiam entidades no banco diretamente via `_factory.Services.CreateScope()` → `AppDbContext`.
- Nomes em inglês, padrão: `MethodName_Scenario_ExpectedResult`
