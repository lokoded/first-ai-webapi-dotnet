---
name: criar-endpoint
description: Use quando precisar criar, adicionar ou implementar um novo endpoint HTTP, rota, action de controller ou método de API. NÃO usar para criar entidades novas ou CRUD completo.
---

## Passo 1 — Identificar o controller

- Recurso autenticado → controller com `[Authorize]`
- Recurso admin → controller separado com `[Authorize(Roles = "Admin")]`
- Recurso público (login, register, health) → sem `[Authorize]`

## Passo 2 — Validar com FluentValidation

Injetar `IValidator<T>` e validar manualmente:

```csharp
var validation = await _validator.ValidateAsync(request);
if (!validation.IsValid)
    return ValidationProblem(validation.ToDictionary());
```

## Passo 3 — Retorno correto

| Método | Retorno |
|--------|---------|
| POST | `CreatedAtAction(nameof(Get), new { id }, response)` |
| PUT | `NoContent()` |
| DELETE | `NoContent()` |
| GET | `Ok(response)` |

NUNCA use `BadRequest()`, `NotFound()` ou `Conflict()`. Use `Problem()` ou `ValidationProblem()`.

## Passo 4 — DI

Services injetados no controller. Repositories NUNCA em controllers.

## Passo 5 — Testes

- Unit test para o service (Moq)
- Integration test para o endpoint (WebApplicationFactory)
- Testar: 200/201, 400, 401, 403, 404
