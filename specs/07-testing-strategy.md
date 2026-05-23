# 07 — Estratégia de Testes

## Stack

- **Framework**: xUnit
- **Mock**: Moq
- **Assertions**: FluentAssertions
- **Integration**: `WebApplicationFactory<Program>` com ambiente "Testing"

## Unit Tests

### Naming
```
MethodName_StateUnderTest_ExpectedBehavior
```
Exemplo: `GetById_WhenComicExists_ReturnsComicResponse`

### Estrutura (AAA)
```csharp
// Arrange
var comicId = Guid.NewGuid();

// Act
var result = await _service.GetByIdAsync(comicId);

// Assert
result.Should().NotBeNull();
result.Id.Should().Be(comicId);
```

### Regras
- Zero I/O real — mockar dependências com Moq
- Cobrir: happy path, not found, null/inválido, conflito
- Nunca `Thread.Sleep()` — abstrair tempo com `ISystemClock` ou similar
- Nunca `[Skip]` sem documentação explícita
- `[Theory]` + `[InlineData]` para múltiplos cenários do mesmo comportamento

## Integration Tests

- `WebApplicationFactory<Program>` com ambiente `"Testing"`
- `[Collection("Database")]` para execução sequencial (compartilham banco)
- Docker obrigatório: SQL Server, Redis, LocalStack
- Testar fluxo real com banco: criar, ler, atualizar, deletar
- Reset de dados entre execuções

## Cobertura de Cenários

Por endpoint, testar:
| Cenário | Status esperado |
|---------|-----------------|
| Sucesso (dados válidos) | 200/201 |
| Dados inválidos | 400 (ValidationProblemDetails) |
| Não autenticado | 401 |
| Sem permissão | 403 |
| Recurso não encontrado | 404 |
| Conflito | 409 |
| Erro de negócio | 422 |

## Threshold Mínimo

- **70% line coverage** (sequence coverage) obrigatório em cada projeto
- No CI, usa-se a **melhor cobertura por projeto entre todas as fontes** (unit + integration)
  - Domain e Application: oriundos dos unit tests (97.2% Application, 85.4% Domain)
  - Infrastructure e WebApi: oriundos dos integration tests (80.5%, 74.2%)
- Exceção progressiva: `FirstWebApi.Application` começa em 60% (mínimo), target 70%
- Verificado no CI via step pós-testes que parseia `coverage.cobertura.xml`
- Comando oficial para validação local:

```powershell
dotnet test --settings coverlet.runsettings
```

- Cobertura atual (23/05/2026) — **melhor fonte por projeto**:
  - Application: 97.2% ✅ (unit)
  - Domain: 85.4% ✅ (unit)
  - Infrastructure: 80.5% ✅ (integration)
  - WebApi: 74.2% ✅ (integration)

## Comandos

```powershell
# Unitários
dotnet test tests/FirstWebApi.UnitTests

# Integração (requer Docker)
dotnet test tests/FirstWebApi.IntegrationTests

# Todos
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```
