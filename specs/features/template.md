# Feature: [Nome da Feature]

## Objetivo

[Descrição de 1 parágrafo do que esta feature faz]

## User Story

```
Como [ator],
quero [ação]
para [benefício].
```

## Contrato da API

### `[METHOD] /api/[rota]`

**Request:**
```json
{
  "campo": "valor"
}
```

**Response `200/201`:**
```json
{
  "id": "guid",
  "propriedade": "valor"
}
```

**Status codes:**

| Status | Quando |
|--------|--------|
| 200/201 | Sucesso |
| 400 | Dados inválidos (validação) |
| 401 | Não autenticado |
| 403 | Sem permissão |
| 404 | Recurso não encontrado |
| 409 | Conflito |

## Regras de Negócio

- [Regra 1]
- [Regra 2]

## Exemplo

### Request
```
[METHOD] /api/[rota]
```

```json
{
  "campo": "exemplo"
}
```

### Response
```
Status: 201 Created
Location: /api/[rota]/<id>
```

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "propriedade": "exemplo"
}
```

## Cenários de Teste

| Cenário | Input | Status esperado |
|---------|-------|-----------------|
| Happy path — dados válidos | [descrição] | 200/201 |
| Campo obrigatório ausente | [descrição] | 400 |
| Recurso não encontrado | [descrição] | 404 |
| Sem autenticação | sem token | 401 |

## Critérios de Aceitação

- [ ] Endpoint implementado conforme contrato
- [ ] Validações de entrada funcionando (400)
- [ ] Autenticação/autorização aplicada
- [ ] Testes unitários passando
- [ ] Testes de integração passando
- [ ] `dotnet build` sem erros
- [ ] `dotnet test` passando
