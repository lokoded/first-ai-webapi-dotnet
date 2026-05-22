# Regras do Projeto — FirstWebApi

> Este arquivo é carregado no contexto do OpenCode em toda sessão.
> Contém as regras, convenções e armadilhas que a IA deve seguir.

---

## Arquitetura

- Clean Architecture com 4 camadas: **Domain → Application → Infrastructure → WebApi**
- SOLID quando fizer sentido, **sem overengineering**
- Nunca adicionar MediatR ou CQRS a menos que explicitamente solicitado
- Nunca adicionar `[Produces("application/json")]` — ASP.NET Core negocia JSON como default
- `SuppressModelStateInvalidFilter = true` em Program.cs — validação é **manual via FluentValidation**

## Convenções de Código

- **Idioma**: Português brasileiro nas strings pro usuário, inglês nos identificadores
- **Namespaces**: file-scoped (C# 10+)
- **Variáveis privadas**: prefixo `_` (ex: `_context`, `_logger`)
- **Preferir `var`** quando o tipo for óbvio
- **Preferir expression-bodied members** quando for uma única expressão
- **Preferir primary constructors** (C# 12+)
- **Sem `[ApiController]` auto-validation** — validar manualmente com FluentValidation
- Usar `Problem()` / `ValidationProblem()` do `ControllerBase`, nunca `BadRequest()`/`NotFound()`/`Conflict()` diretamente

## Estrutura de Pastas

```
FirstWebApi.slnx
├── src/FirstWebApi.Domain/           # Entities, ValueObjects, Enums, Interfaces (repository only)
├── src/FirstWebApi.Application/      # DTOs/Request/, DTOs/Response/, Services/, Validators/
├── src/FirstWebApi.Infrastructure/   # Data/, Repositories/, Services/, Data/Configurations/, Data/Migrations/
└── src/FirstWebApi.WebApi/           # Controllers/, Middleware/, Logging/, Properties/
tests/
├── FirstWebApi.UnitTests/            # Domain/, Application/, Infrastructure/
└── FirstWebApi.IntegrationTests/     # Controllers/
```

## Domain

- Nenhuma dependência NuGet exceto `Microsoft.Extensions.Identity.Stores`
- Value Objects são `readonly struct` com validação própria (ex: `Cpf`, `Email`)
- Entidades: `User` extends `IdentityUser<Guid>`, `Comic`, `ComicType`, `Address`
- Repository interfaces apenas (implementação na Infrastructure)
- `IUnitOfWork` com `Task<int> CommitAsync()`

## Application

- DTOs de request em `DTOs/Request/`, response em `DTOs/Response/`
- Validators usam `FluentValidation` — um validator por request
- Services chamam repositories, aplicam regras de negócio, retornam DTOs
- Nunca expor entidades do Domain nos controllers
- `IEncryptionService` para criptografia LGPD (CPF, RG, endereço)

## Infrastructure

- EF Core: `AppDbContext` extends `IdentityDbContext<User, IdentityRole<Guid>, Guid>`
- Configurações Fluent API em `Data/Configurations/` (classes separadas por entidade)
- Repositories implementam interfaces do Domain
- Decorators de cache Redis via Scrutor (padrão decorator, cache-aside)
- KMS: envelope encryption (GenerateDataKey + AES-256-CBC)
- JWT: HMAC-SHA256, 8h expiry

## WebApi / Controllers

- **TODO controller** injeta `IValidator<T>` do request e valida manualmente:
  ```csharp
  var validation = await _validator.ValidateAsync(request);
  if (!validation.IsValid)
      return ValidationProblem(validation.ToDictionary());
  ```
- Retornar `Created()` para POST, `Ok()` para GET, `NoContent()` para PUT/DELETE
- `[Authorize]` global no controller, não por action (exceto quando público)
- Admin endpoints ficam em controller separado com `[Authorize(Roles = "Admin")]`
- Health check em controller separado ou middleware, sem auth

## Testes

- **Unit tests**: Moq para mockar dependências, testar lógica de serviço
- **Integration tests**: `WebApplicationFactory<Program>` com ambiente "Testing"
- Usar `[Collection("Database")]` para testes sequenciais de banco
- FluentAssertions para todas as assertions
- Testar cenários de sucesso E falha (401, 403, 404, 409, 422)

## Segurança

- CPF, RG e endereço são **sempre cifrados** via `IEncryptionService` — nunca em texto puro
- Logs **nunca** contêm secrets, CPF, RG ou dados sensíveis
- Exception middleware nunca expõe stack trace
- Configurações sensíveis via `.env` (não versionado)
- Rate limiting deve ser considerado em endpoints de auth
- Refresh tokens para renovação segura de sessão

## Workflow de Branches

- Toda branch `feature/*` ou `fix/*` parte da `develop` e faz PR para `develop`
- `hotfix/*` parte da `main` e faz PR para `main` (correção urgente em produção)
- `release/*` parte da `develop` e faz PR para `main`
- `main` só recebe merge via `develop` → `main` ou `hotfix/*` → `main`
- Após merge de `hotfix/*` → `main`, fazer merge de volta para `develop`
- Após qualquer PR mergir em `main`, sincronizar `develop` via `git rebase main` ou `git reset --hard main`
- Branches merged são deletadas local e remotamente — `git branch -d` e `git push origin --delete`
- Exceção: branches de longo prazo (`develop`, `main`)

## Workflow de Commits

- Ao marcar um todo como `completed`, executar automaticamente:
  1. `git add -A`
  2. `git commit -m "<tipo>: <descrição>"` seguindo **Conventional Commits**
- Tipos derivados do contexto do todo:
  - `feat` — nova funcionalidade
  - `fix` — correção de bug
  - `refactor` — refatoração
  - `test` — testes
  - `chore` — manutenção, config, dependências
  - `docs` — documentação
  - `perf` — performance
  - `style` — formatação, linting
- Exemplo: todo *"Criar endpoint de login"* → commit `feat: criar endpoint de login`
- Se não houver mudanças a commitar, pular silenciosamente
- Se o commit falhar, reportar o erro ao usuário e não prosseguir
- Ao marcar o **último** todo como `completed`, perguntar ao usuário: *"Deseja fazer push?"* — se sim, executar `git push`

## Armadilhas Conhecidas (LEIA ANTES DE ALTERAR)

1. `SuppressModelStateInvalidFilter = true` — SEMPRE validar com FluentValidation manualmente
2. `KmsEncryptionService` inicialização é lazy (`Lazy<Task>`) — primeira chamada pode ser lenta se LocalStack estiver offline
3. Paginação do `ComicService.GetAllAsync` usa OFFSET/FETCH no banco — NÃO é mais em memória
4. `Tag` no AES-GCM tem 16 bytes — diferente do CBC antigo (Array.Empty)
5. Migration automática só roda em Development/Testing — em produção usar script idempotente
6. `User.Role` removido — autorização usa exclusivamente `ClaimTypes.Role` do Identity
7. Integration tests compartilham banco `FirstWebApiDb_Test` — rodar em sequência (Collection)
8. `appsettings.Testing.json` contém credenciais de desenvolvimento (banco local Docker) versionadas — CI sobrescreve com `secrets.*`. Isso é seguro porque as credenciais só funcionam no container Docker local.
9. PRs de `feature/*` e `fix/*` devem SEMPRE targetar `develop`, nunca `main`. `main` só recebe PR de `develop` ou `hotfix/*`.
