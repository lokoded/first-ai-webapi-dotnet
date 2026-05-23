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
