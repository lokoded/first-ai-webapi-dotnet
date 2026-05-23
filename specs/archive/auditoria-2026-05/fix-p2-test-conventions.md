# Fix: Testes — Naming, Dead Code, Theory Consolidation (P2)

## Objetivo

Corrigir convenções de teste: naming de método em UserTests, remover código morto/duplicado, consolidar testes de validação em `[Theory]`.

## Regras de Negócio

1. `UserTests.CreateUser_ShouldHaveValidId` → `CreateUser_WithValidData_ShouldHaveValidId`
2. `TokenServiceTests`: remover linhas em branco duplicadas
3. `DatabaseCollection`: remover `ICollectionFixture<FirstWebApiFactory>` não utilizado
4. `IntegrationTestBase.SetAuthHeader`: remover código morto ou refatorar callers
5. `AuthControllerTests` (após refatorar para IntegrationTestBase): consolidar 4x `[Fact]` de validação 400 em `[Theory]`

## Arquivos Afetados

- `tests/FirstWebApi.UnitTests/Domain/UserTests.cs`
- `tests/FirstWebApi.UnitTests/Infrastructure/TokenServiceTests.cs`
- `tests/FirstWebApi.IntegrationTests/DatabaseCollection.cs`
- `tests/FirstWebApi.IntegrationTests/IntegrationTestBase.cs`
- `tests/FirstWebApi.IntegrationTests/Controllers/AuthControllerTests.cs`

## Critérios de Aceitação

- [ ] Naming de UserTests segue padrão `MethodName_StateUnderTest_ExpectedBehavior`
- [ ] TokenServiceTests sem linhas em branco extras
- [ ] DatabaseCollection sem ICollectionFixture não utilizado
- [ ] SetAuthHeader removido ou em uso
- [ ] AuthControllerTests usa `[Theory]` para cenários de 400
- [ ] `dotnet test` passa
