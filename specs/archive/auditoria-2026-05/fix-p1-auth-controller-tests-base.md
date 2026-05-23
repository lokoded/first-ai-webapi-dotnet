# Fix: AuthControllerTests — Herdar IntegrationTestBase (P1)

## Objetivo

Refatorar `AuthControllerTests` para estender `IntegrationTestBase`, eliminando duplicação de `_client` e `JsonContent`.

## Regras de Negócio

1. `AuthControllerTests` deve herdar de `IntegrationTestBase`
2. Remover campo `_client` duplicado (usar herdado)
3. Remover método `JsonContent` duplicado (usar herdado)
4. Ajustar chamadas para usar `Client` e helpers do base

## Arquivos Afetados

- `tests/FirstWebApi.IntegrationTests/Controllers/AuthControllerTests.cs`

## Critérios de Aceitação

- [ ] AuthControllerTests herda IntegrationTestBase
- [ ] Código duplicado removido
- [ ] Todos os testes de auth continuam passando
- [ ] `dotnet test` passa
