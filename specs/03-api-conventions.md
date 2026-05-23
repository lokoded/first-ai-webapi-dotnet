# 03 — Convenções de API

## HTTP Status Codes

| Método | Sucesso | Observações |
|--------|---------|-------------|
| GET | `200 OK` | Retorna o recurso ou lista |
| POST | `201 Created` | + header `Location` com URL do recurso |
| PUT | `204 No Content` | Recurso atualizado |
| DELETE | `204 No Content` | Recurso removido |

Erros (sempre RFC 9457 — `application/problem+json`):

| Status | Quando |
|--------|--------|
| 400 | Erro de validação (auto-validation do `[ApiController]`) |
| 401 | Não autenticado |
| 403 | Autenticado, sem permissão |
| 404 | Recurso não encontrado |
| 409 | Conflito (ex: recurso duplicado) |
| 422 | Erro de negócio (quando 400 não é suficiente) |
| 500 | Erro interno não esperado (nunca expor detalhes) |

NUNCA retornar 200 com corpo de erro. NUNCA usar `BadRequest()`, `NotFound()` ou `Conflict()` diretamente — usar `Problem()` ou `ValidationProblem()`.

## Auto-validation

- `[ApiController]` validará automaticamente os requests com FluentValidation
- NÃO injetar `IValidator<T>` nos controllers
- NÃO ter blocos `if (!validation.IsValid)`
- Resposta de erro: `ValidationProblemDetails` com campo `errors` (dict de campo → mensagens)

## Paginação

- Padrão: 20 itens por página, máximo: 100
- Query params: `?page=1&pageSize=20`
- Resposta inclui metadados: `{ data: [...], page, pageSize, totalCount, totalPages }`
- Filtragem e ordenação no banco (OFFSET/FETCH), nunca em memória

## Retornos

- GET lista paginada: `Ok(PagedResponse<T>)`
- GET por ID: `Ok(response)` ou `Problem(status: 404)`
- POST: `CreatedAtAction(nameof(Get), new { id }, response)`
- PUT: `NoContent()`
- DELETE: `NoContent()`

## Location Header

Sempre retornar `Location` em POST via `CreatedAtAction` ou `CreatedAtRoute`.

## Convenções de Rota

- Plural: `/api/comics`, `/api/users`
- Aninhado quando faz sentido: `/api/users/{userId}/address`
- Versionamento: header `Accept: application/vnd.firstwebapi.v1+json` ou via URL (`/api/v1/...`) — decidir por ADR quando necessário

## ProducesResponseType

Todos os endpoints documentam retornos com `[ProducesResponseType(typeof(T), StatusCodes.Status200OK)]` para Swagger/OpenAPI.
