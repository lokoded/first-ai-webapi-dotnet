---
name: criar-endpoint
description: Use quando precisar criar, adicionar ou implementar um novo endpoint HTTP, rota, action de controller ou método de API. NÃO usar para criar entidades novas ou CRUD completo.
---

## Passo 1 — Verificar Spec Existente

Se houver feature spec em `specs/features/`, segui-la rigorosamente (contrato, regras, status codes).

## Passo 2 — Identificar o Controller

- Recurso autenticado → controller com `[Authorize]`
- Recurso admin → controller separado com `[Authorize(Roles = "Admin")]`
- Recurso público (login, register, health) → sem `[Authorize]`

## Passo 3 — Auto-validation

O `[ApiController]` valida automaticamente com FluentValidation. NÃO injetar `IValidator<T>` no controller. NÃO ter blocos `if (!validation.IsValid)`.

## Passo 4 — Retorno Correto

| Método | Retorno |
|--------|---------|
| POST | `CreatedAtAction(nameof(Get), new { id }, response)` |
| PUT | `NoContent()` |
| DELETE | `NoContent()` |
| GET | `Ok(response)` |

NUNCA use `BadRequest()`, `NotFound()` ou `Conflict()`. Use `Problem()` ou `ValidationProblem()`.

## Passo 5 — DI

Services injetados no controller. Repositories NUNCA em controllers.

## Passo 6 — Testes

- Unit test para o service (Moq)
- Integration test para o endpoint (WebApplicationFactory)
- Testar: 200/201, 400, 401, 403, 404, 409 conforme especificado

## Referências

- `specs/03-api-conventions.md` — HTTP codes, auto-validation, paginação
- `specs/01-coding-standards.md` — convenções de código
