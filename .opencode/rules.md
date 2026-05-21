# Regras do Projeto — FirstWebApi

> Este arquivo é carregado no contexto do OpenCode em toda sessão.
> Contém as regras, convenções e armadilhas que a IA deve seguir.
> Mantenha atualizado conforme o projeto evolui.

---

## Arquitetura

- Clean Architecture com 4 camadas: **Domain → Application → Infrastructure → WebApi**
- SOLID quando fizer sentido, **sem overengineering**
- Nunca adicionar MediatR ou CQRS a menos que explicitamente solicitado
- Nunca adicionar `[Produces("application/json")]` — ASP.NET Core negocia JSON como default
- `SuppressModelStateInvalidFilter = true` em Program.cs — validação é **manual via FluentValidation**

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 10 |
| ORM | EF Core 10 + SQL Server |
| Cache | Redis (StackExchange) |
| Criptografia | AWS KMS (LocalStack) + AES-256-CBC envelope |
| Auth | JWT HMAC-SHA256 + ASP.NET Identity |
| Docs | Swagger (dev) + Scalar |
| Validação | FluentValidation |
| Testes | xUnit + Moq + FluentAssertions + WebApplicationFactory |

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

## Armadilhas Conhecidas (LEIA ANTES DE ALTERAR)

1. `SuppressModelStateInvalidFilter = true` — SEMPRE validar com FluentValidation manualmente
2. `KmsEncryptionService.InitializeKeyAsync().GetAwaiter().GetResult()` — trava se LocalStack estiver offline
3. Paginação do `ComicService.GetAllAsync` é **em memória** — precisa migrar para OFFSET/FETCH no banco
4. `Tag` no AES-CBC é sempre vazio (`Array.Empty<byte>()`) — não usar tag em CBC
5. Migration automática só roda em Development/Testing — em produção usar script idempotente
6. `User.Role` (EUserRole) existe no banco mas autorização usa `ClaimTypes.Role` do Identity — campos semi-duplicados
7. Integration tests compartilham banco `FirstWebApiDb_Test` — rodar em sequência (Collection)
8. `.env` template existe no repositório mas valores reais estão no `.gitignore`

## Comandos Essenciais

```powershell
dotnet build                                   # Compilar
dotnet test tests/FirstWebApi.UnitTests        # Testes unitários (rápidos, sem infra)
dotnet test tests/FirstWebApi.IntegrationTests # Testes integração (requer docker)
dotnet test                                    # Todos os testes
docker compose up -d                           # Infra completa
docker compose down -v                         # Destrói tudo (dados inclusive)
```
