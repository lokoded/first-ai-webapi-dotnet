# Fix: PaginatedResult — Alinhar Contrato da API (P1)

## Objetivo

Corrigir `PaginatedResult` para serializar `data` em vez de `items`, e remover `[JsonIgnore]` de `TotalPages`.

## Regras de Negócio

1. Propriedade `Items` deve serializar como `"data"` (via `[JsonPropertyName("data")]` ou renomeando para `Data`)
2. `TotalPages` não deve ter `[JsonIgnore]` — campo é parte do contrato

## Arquivo Afetado

- `src/FirstWebApi.Application/DTOs/Response/PaginatedResult.cs`

## Critérios de Aceitação

- [ ] Resposta paginada contém `"data"` em vez de `"items"`
- [ ] Resposta paginada contém `"totalPages"`
- [ ] Testes existentes continuam passando
- [ ] `dotnet build` passa
