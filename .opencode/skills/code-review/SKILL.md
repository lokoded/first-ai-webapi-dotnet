---
name: code-review
description: Use quando precisar revisar código, verificar qualidade, auditar segurança ou validar PRs. Aborda OWASP Top 10, Clean Architecture, convenções de código e armadilhas conhecidas da FirstWebApi.
---

## O que verificar (por ordem)

### 1. Segurança
- Dados sensíveis cifrados via `IEncryptionService`?
- Logs expõem secrets ou dados pessoais?
- Auth necessária aplicada?
- Validação FluentValidation presente?

### 2. Arquitetura
- Dependências seguem Clean Architecture?
- Controller injeta repository? → Erro, deve injetar service
- Controller expõe entidade? → Erro, deve usar DTO

### 3. Código
- `Problem()`/`ValidationProblem()` no lugar de `BadRequest()`/`NotFound()`?
- Tratamento de erro adequado?
- Testes unitário + integração?
- Segue `.editorconfig`?

### 4. Performance
- Paginação OFFSET/FETCH (não em memória)?
- Cache Redis para leituras frequentes?
- Async/await sem `.Result` ou `.Wait()`?

### 5. Peculiaridades
- `SuppressModelStateInvalidFilter = true`? Validação manual obrigatória
- Migration nova? Script idempotente necessário
- KMS depende de LocalStack? Testar com LocalStack rodando

## Checklist Final
- [ ] Build passa (0 erros, 0 warnings)?
- [ ] Testes unitários passam?
- [ ] Testes de integração passam?
- [ ] Dados sensíveis protegidos?
- [ ] Validação implementada?
- [ ] Retornos RFC 9457?
