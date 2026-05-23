# Auditoria de Código — FirstWebApi

**Data:** 2026-05-22  
**Escopo:** Full codebase (~4.490 LOC, 85 arquivos .cs)  
**Metodologia:** Revisão manual por IA com análise de 6 camadas em paralelo  
**Critérios:** Código em local errado, não utilizado, feito errado, lógica exagerada, redundância, defasado, soluções exageradas

---

## Sumário Executivo

| Métrica | Valor |
|---------|-------|
| Issues encontradas | **30** (2 críticas, 8 altas, 12 médias, 8 baixas) |
| Camada com mais issues | Infrastructure (8) + Tests (7) |
| Prioridade máxima | Cache Redis corrompido + cache de update nunca invalida |
| Arquivo mais problemático | `AuthService.cs` (5 issues) |

### Distribuição por Camada

| Camada | Crítico | Alta | Média | Baixa | Total |
|--------|---------|------|-------|-------|-------|
| Domain | 0 | 2 | 3 | 2 | 7 |
| Application | 0 | 4 | 2 | 1 | 7 |
| Infrastructure | 2 | 1 | 2 | 2 | 7 |
| WebApi | 0 | 2 | 3 | 2 | 7 |
| Tests (Unit + Integration) | 0 | 2 | 5 | 2 | 9 |

> Nota: Algumas issues aparecem em múltiplas camadas (ex: Email VO não usado afeta Domain + Application + Tests).

---

## Issues por Camada

### 1. Domain Layer (`src/FirstWebApi.Domain/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| D01 | 🔴 Alta | Código não utilizado | `ValueObjects/Email.cs` | Arquivo inteiro | ValueObject `Email` definido, testado, mas **nunca usado em produção**. CPF usa ValueObject (`AuthService.cs:52`), Email não. Validação de email é feita exclusivamente via FluentValidation `.EmailAddress()`. Inconsistente com o padrão do CPF. |
| D02 | 🔴 Alta | Código feito errado | `ValueObjects/Cpf.cs` | 3 | `readonly record struct` permite `default(Cpf)` — instância inválida com `Numero = null`, mesmo com nullable habilitado. O construtor não é chamado. Mesmo problema em `Email.cs:5`. |
| D03 | 🟡 Média | Código em local errado | `Entities/User.cs` | 9-17 | Detalhes de infraestrutura de criptografia (Ciphertext, IV, Tag, EncryptedDataKey) vazam para o Domain. Se o algoritmo mudar (ex: AES-CBC → AES-GCM), o Domain precisa ser alterado. Mesmo problema em `Address.cs:9-12`. |
| D04 | 🟡 Média | Inconsistência | `Entities/ComicType.cs` | 7 | Não possui `UpdatedAt` — diferente de User, Comic, Address, RefreshToken. Impossível rastrear modificações. |
| D05 | 🟡 Média | Código defasado | `Interfaces/IUnitOfWork.cs` | 5 | Documentação diz `CommitAsync()`, código real é `SaveChangesAsync()`. Implementação implícita frágil (confia que assinatura do EF Core sempre vai bater). |
| D06 | 🟢 Baixa | Código feito errado | `ValueObjects/Cpf.cs` | 27 | `int.Parse(c.ToString())` ineficiente — prefira `c - '0'`. |
| D07 | 🟢 Baixa | Inconsistência | `Entities/Address.cs` | 30-31 | `UpdatedAt` setado no mesmo timestamp de `CreatedAt` na criação — semanticamente impreciso. |

### 2. Application Layer (`src/FirstWebApi.Application/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| A01 | 🔴 Alta | Código feito errado | `Services/AuthService.cs` | 31,36,45,92,96,103,127,133,137 | Uso de `InvalidOperationException` e `UnauthorizedAccessException` genéricas para regras de negócio. Força middleware a inspecionar mensagem para determinar HTTP status code. |
| A02 | 🔴 Alta | Código feito errado | `Services/AuthService.cs` | 43-45 | Identity error messages (`result.Errors.Select(e => e.Description)`) expostas ao cliente — information disclosure. Podem revelar política de senha. |
| A03 | 🔴 Alta | Código feito errado | `Services/ComicService.cs` | 89 | `comic.ComicType?.Nome` depende de `.Include()` implícito no repositório — se esquecer, bug silencioso (string vazia). |
| A04 | 🟡 Média | Código em local errado | `Services/ProfileService.cs` | 120-140 | Métodos `MaskCpf()`, `MaskRg()`, `MaskEndereco()` são lógica de apresentação na camada errada. Deveria estar no WebApi (mapping/viewmodel). |
| A05 | 🟡 Média | Redundância | `Services/AuthService.cs` | 72-74,108-110,146-148 | Criação de refresh token duplicada 3 vezes. Extrair para método privado `CreateRefreshTokenAsync()`. |
| A06 | 🟡 Média | Redundância | `Services/AuthService.cs` | 78-79,113-114,151-152 | Geração de token + roles duplicada 3 vezes. Extrair para método privado. |
| A07 | 🟢 Baixa | Lógica grande | `Services/AuthService.cs` | 28-84 | `RegisterAsync()` com 56 linhas — validação + criptografia CPF/RG/endereço + refresh token + response. Extrair helpers. |

### 3. Infrastructure Layer (`src/FirstWebApi.Infrastructure/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| I01 | 🔴 Crítico | Código feito errado | `Repositories/Decorators/CachedComicRepository.cs` | 54-67 | **Cache de update nunca é invalidado.** `ComicService.UpdateAsync` nunca chama `comicRepository.UpdateAsync()` — modifica entidade tracked e chama só `SaveChangesAsync`. Cache individual (`comic:{id}`) e paginado (`comics:user:{userId}:page:*`) ficam stale após updates. |
| I02 | 🔴 Crítico | Código feito errado | `Repositories/Decorators/CachedComicRepository.cs` + `Domain/Entities/Comic.cs` | 28,43 | **Entidades não podem ser desserializadas do Redis.** `Comic` e `ComicType` têm `private set` em todas as propriedades. `System.Text.Json` (default) não consegue setá-las. Resultado: `Id = Guid.Empty`, `CreatedAt = DateTime.MinValue`, dados corrompidos. |
| I03 | 🔴 Alta | Código não utilizado | `Repositories/ComicRepository.cs` | 41-45 | `UpdateAsync()` é **dead code** — nunca chamado por ninguém. `ComicService.UpdateAsync` modifica entidade tracked e chama só `unitOfWork.SaveChangesAsync()`. |
| I04 | 🟡 Média | Código feito errado | `Repositories/ComicRepository.cs` | 41-45 | Se fosse chamado, `DbSet.Update()` em entidade já tracked marcaria TODAS propriedades como modified — SQL UPDATE desnecessário de todas as colunas. |
| I05 | 🟡 Média | Redundância | `Repositories/Decorators/CachedComicRepository.cs` + `CachedComicTypeRepository.cs` | 18-21 | `JsonSerializerOptions` inconsistentes. `CachedComicRepository` usa `CamelCase`, `CachedComicTypeRepository` usa default (`PascalCase`). Dados em cache com naming diferentes. |
| I06 | 🟡 Média | Código defasado | `Repositories/RefreshTokenRepository.cs` | 37-40 | `Update()` é síncrono (`void`) — inconsistente com todos os outros métodos que retornam `Task`. |
| I07 | 🟢 Baixa | Código feito errado | `Repositories/Decorators/CachedComicRepository.cs` | 76 | `_redis.GetEndPoints()[0]` — assume que há pelo menos um endpoint. `IndexOutOfRangeException` se Redis mal configurado. |
| I08 | 🟢 Baixa | Código defasado | `Repositories/RefreshTokenRepository.cs` | 24-30 | `DeleteExpiredAsync` nome enganoso — não deleta todos expirados, só os mais velhos que `keepDays`. |

### 4. WebApi Layer (`src/FirstWebApi.WebApi/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| W01 | 🔴 Alta | Código em local errado | `Logging/FileLogger.cs` | Arquivo inteiro (120 loc) | Implementação completa de `ILoggerProvider` na WebApi. Deveria estar em Infrastructure. |
| W02 | 🔴 Alta | Código feito errado | `Middleware/ExceptionMiddleware.cs` | 32 | `InvalidOperationException` vaza `exception.Message` real no response 500 — information disclosure. Outras exceções usam mensagem genérica. |
| W03 | 🟡 Média | Redundância | `Controllers/*.cs` (4 lugares) | `AuthController.cs:58-61`, `ComicsController.cs:22-27`, `UsersController.cs:23-26`, `UsersController.cs:44-47` | Extração de `UserId` do `ClaimsPrincipal` com fallback `"sub"` duplicada em 4 lugares com padrões diferentes. Criar extension method `ClaimsPrincipal.GetUserId()`. |
| W04 | 🟡 Média | Código feito errado | `Controllers/UsersController.cs` | 53-64 | `UnauthorizedAccessException` capturada no controller retorna 403, mas middleware mapeia como 401 — inconsistência. |
| W05 | 🟡 Média | Código feito errado | `Controllers/AdminComicTypesController.cs` | 34 | URL hardcoded em `Created($"/api/admin/comic-types/{id}")` — quebra se `[Route]` mudar. |
| W06 | 🟢 Baixa | Código feito errado | `Controllers/ComicsController.cs` | 38,52,70,89,110 | Magic number `401` em vez de `StatusCodes.Status401Unauthorized`. `UsersController` usa o correto. |
| W07 | 🟢 Baixa | Código não utilizado | `Controllers/AuthController.cs`, `ComicsController.cs`, `AdminComicTypesController.cs` | 7,8,7 | `using Microsoft.AspNetCore.Mvc.ModelBinding` não utilizado — `ValidationProblemDetails` está em `Microsoft.AspNetCore.Mvc`. |
| W08 | 🟢 Baixa | Redundância | `Controllers/*.cs` (6 lugares) | Várias | `ValidationProblem(validation.ToDictionary())` repetido 7 vezes. Poderia ser helper. |

### 5. Unit Tests (`tests/FirstWebApi.UnitTests/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| U01 | 🔴 Alta | Código não utilizado | `Infrastructure/KmsEncryptionServiceTests.cs` | 15,21-43 | `_kmsMock` é criado com setup elaborado mas **nunca injetado** no `KmsEncryptionService`. O construtor aceita `IAmazonKeyManagementService? kmsClient = null` — passar null faz o serviço criar cliente real contra LocalStack. Testes são acidentalmente integração. |
| U02 | 🔴 Alta | Código feito errado | `Infrastructure/KmsEncryptionServiceTests.cs` | 45-113 | Testes dependem de LocalStack real rodando. Se LocalStack estiver offline, falham com connection error — não testam lógica. |
| U03 | 🟡 Média | Solução exagerada | `Infrastructure/KmsEncryptionServiceTests.cs` | 19-43 | Setup super elaborado (6 configs appsettings, 2 mocks, logger) mas todo inútil porque mock não é injetado. |
| U04 | 🟡 Média | Redundância | `Application/ComicServiceTests.cs` | 103-160 | Criação de entidade `Comic` duplicada 4+ vezes com mesmos parâmetros. Extrair factory method. |
| U05 | 🟡 Média | Redundância | `Domain/UserTests.cs` + `Application/*Tests.cs` + `Infrastructure/*Tests.cs` | Várias | Testes do `Email` VO existem, mas VO não é usado em produção — testa código morto. |
| U06 | 🟢 Baixa | Código feito errado | `Application/ComicServiceTests.cs` | 84-90 | Mock de `GetByIdAsync` setup após callback de `AddAsync` — código morto no teste (`CreateAsync` não chama `GetByIdAsync`). |

### 6. Integration Tests (`tests/FirstWebApi.IntegrationTests/`)

| # | Severidade | Categoria | Arquivo | Linha | Descrição |
|---|-----------|-----------|---------|-------|-----------|
| IT01 | 🟡 Média | Código não utilizado | `FirstWebApi.IntegrationTests.csproj` | 18 | Dependência `Moq` nunca usada — testes usam `WebApplicationFactory` com banco real. |
| IT02 | 🟡 Média | Redundância | `Controllers/*Tests.cs` (3 arquivos) | `ComicsControllerTests.cs:26-50`, `ComicTypesControllerTests.cs:26-50`, `UserControllerTests.cs:26-51` | `RegisterAndGetTokenAsync()` idêntico copiado em 3 arquivos. Extrair para `FirstWebApiFactory` ou base class. |
| IT03 | 🟡 Média | Lógica grande | `Controllers/ComicTypesControllerTests.cs` | 122-165 | `RegisterAndGetAdminTokenAsync()` com 44 linhas sequenciais — registro + DI scope + admin role + re-login. Duplica maioria de `RegisterAndGetTokenAsync()`. |
| IT04 | 🟢 Baixa | Redundância | `Controllers/*Tests.cs` | ~20+ ocorrências | `new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")` repetido. Helper `JsonContent.Create()` eliminaria. |
| IT05 | 🟢 Baixa | Código feito errado | `Controllers/ComicTypesControllerTests.cs` | 167-183 | Teste de admin POST só asserta status 201, nunca verifica response body. Passaria mesmo com resposta vazia. |

---

## Issues por Categoria

| Categoria | Crítico | Alta | Média | Baixa | Total |
|-----------|---------|------|-------|-------|-------|
| Código em local errado | 0 | 1 | 3 | 0 | 4 |
| Código não utilizado | 0 | 3 | 1 | 1 | 5 |
| Código feito errado | 2 | 4 | 3 | 3 | 12 |
| Lógica grande/exagerada | 0 | 0 | 2 | 1 | 3 |
| Redundância | 0 | 0 | 4 | 2 | 6 |
| Código defasado | 0 | 0 | 2 | 0 | 2 |
| Solução exagerada | 0 | 0 | 1 | 0 | 1 |
| Inconsistência | 0 | 0 | 2 | 1 | 3 |

---

## Plano de Correção Recomendado

### Fase 1 — Críticos (corrigir imediatamente)

| Ordem | Issue | Ação | Arquivos afetados | Risco |
|-------|-------|------|-------------------|-------|
| 1 | I01 — Cache de update nunca invalida | `ComicService.UpdateAsync` deve chamar `comicRepository.UpdateAsync(comic)` para disparar o decorator | `ComicService.cs`, `ComicRepository.cs` | Médio — pode causar dirty writes se não testado |
| 2 | I02 — Entidades não desserializam do Redis | Opção A: Usar DTOs no cache em vez de entidades. Opção B: Adicionar `[JsonInclude]` + `[JsonConstructor]` nas entidades. Opção C: Public setters. **Recomendado: Opção A (DTOs)** | `CachedComicRepository.cs`, `CachedComicTypeRepository.cs`, possivelmente `Comic.cs`, `ComicType.cs` | Alto — mexe no contrato de cache |

### Fase 2 — Altos (corrigir em seguida)

| Ordem | Issue | Ação | Arquivos afetados |
|-------|-------|------|-------------------|
| 3 | W01 — FileLogger na WebApi | Mover para `Infrastructure/Logging/` | `FileLogger.cs`, `Program.cs` (atualizar using/DI) |
| 4 | D01 — Email VO nunca usado | Decidir: remover ou integrar. Se integrar, usar no `AuthService` igual CPF | `Email.cs`, `AuthService.cs`, `RegisterRequestValidator.cs` |
| 5 | A01 — Exceções genéricas | Criar exceções específicas: `DuplicateEmailException`, `AuthenticationFailedException`, `UserNotFoundException` | `AuthService.cs`, `ExceptionMiddleware.cs` |
| 6 | A02 — Identity errors expostos | Logar erros detalhados, lançar mensagem genérica | `AuthService.cs` |
| 7 | W02 — Exception vaza mensagem | Substituir por mensagem genérica como nas outras exceções | `ExceptionMiddleware.cs` |
| 8 | U01+U02 — KMS mock não injetado | Passar `_kmsMock.Object` no construtor (parâmetro opcional já existe) | `KmsEncryptionServiceTests.cs` |
| 9 | D02 — default(Cpf) inválido | Converter para `readonly record struct` com validação no acesso, ou mudar para `record class` | `Cpf.cs`, `Email.cs` |

### Fase 3 — Médios (corrigir quando possível)

| Ordem | Issue | Ação |
|-------|-------|------|
| 10 | W03 — UserId duplicado | Extension method `ClaimsPrincipal.GetUserId()` |
| 11 | A05+A06 — Refresh token e token gen duplicados | Private helpers em `AuthService` |
| 12 | IT02 — RegisterAndGetTokenAsync duplicado | Extrair para base class ou `FirstWebApiFactory` |
| 13 | W04 — 401 vs 403 inconsistente | Alinhar middleware + controller |
| 14 | D03 — Criptografia vaza no Domain | Value Object `EncryptedData` ou blob único |
| 15 | I03 — `UpdateAsync` dead code | Decidir se mantém (para consistência) ou remove |
| 16 | I05 — JsonSerializerOptions inconsistente | Unificar para mesma policy |
| 17 | I06 — Update síncrono | Mudar para `Task` |
| 18 | IT01 — Moq não usado | Remover package reference |
| 19 | D04 — ComicType sem UpdatedAt | Adicionar propriedade |
| 20 | D05 — IUnitOfWork naming | Alinhar doc com código ou vice-versa |
| 21 | A04 — Masking no Application | Mover para WebApi (DTO mapping) |
| 22 | U03 — Setup exagerado | Simplificar após corrigir injeção do mock |
| 23 | U04 — Comic duplicado | Factory method |
| 24 | IT03 — AdminToken 44 linhas | Extrair helper |

### Fase 4 — Baixos (nice to have)

| Ordem | Issue | Ação |
|-------|-------|------|
| 25 | W06 — Magic number 401 | Substituir por `StatusCodes.Status401Unauthorized` |
| 26 | W07 — Using não utilizados | Remover |
| 27 | D06 — int.Parse ineficiente | `c - '0'` |
| 28 | D07 — UpdatedAt na criação | Não setar UpdatedAt se é criação |
| 29 | I07 — GetEndPoints sem fallback | Adicionar guarda |
| 30 | W08 — ValidationProblem duplicado | Helper opcional |

---

## Dependências entre Correções

```
I01 (cache update) ← independente
  └── depende de: entender fluxo ComicService.UpdateAsync
  
I02 (cache JSON) ← independente
  └── dependente de: decisão arquitetural (DTO vs JsonInclude vs public setters)

A01 (exceções) → W02 (exception middleware)
  └── W02 precisa saber os novos tipos de exceção para mapear corretamente

U01+U02 (KMS mock) ← independente
  └── correção simples: passar mock no construtor existente

W01 (FileLogger) ← independente
  └── mover arquivo + atualizar DI em Program.cs

D01 (Email VO) → A05? (depende da decisão: remover ou integrar)
```

---

## Status das Correções

| Data | Issue | Status | Observação |
|------|-------|--------|------------|
| — | — | Pendente | Nenhuma correção iniciada |

*Atualize esta tabela conforme as correções forem feitas.*

---

## Comandos Úteis para Verificação

```powershell
# Verificar se código compila
dotnet build

# Rodar testes de unidade
dotnet test tests/FirstWebApi.UnitTests

# Rodar testes de integração (requer Docker UP)
docker compose ps
dotnet test tests/FirstWebApi.IntegrationTests

# Verificar warnings do compilador
dotnet build -warnaserror

# Verificar código não utilizado (análise estática)
dotnet tool install -g dotnet-consolidate 2>$null
```

---

## Histórico de Auditorias

| Data | Versão | Auditor | Escopo |
|------|--------|---------|--------|
| 2026-05-22 | 1.0 | opencode/dev-agent | Full codebase — 30 issues encontradas |

*Mantenha este histórico atualizado para rastrear progresso ao longo do tempo.*
