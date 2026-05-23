---
name: code-review
description: Use quando precisar revisar código, verificar qualidade, auditar segurança ou validar PRs. Aborda OWASP Top 10, Clean Architecture, coding standards e specs do projeto.
---

## O que verificar (por ordem)

### 1. Segurança
- Dados sensíveis cifrados via `IEncryptionService`?
- Logs expõem secrets ou dados pessoais?
- Auth necessária aplicada? Ownership validado?
- FluentValidation presente nos DTOs de request?

### 2. Arquitetura
- Dependências seguem Clean Architecture?
- Controller injeta repository? → Erro, deve injetar service
- Controller expõe entidade? → Erro, deve usar DTO
- Segue `specs/00-architecture.md`?

### 3. Código
- `Problem()`/`ValidationProblem()` no lugar de `BadRequest()`/`NotFound()`?
- Auto-validation do `[ApiController]` (NÃO validar manualmente)
- Segue `specs/01-coding-standards.md`?
- Testes unitários + integração?

### 4. Performance
- Paginação OFFSET/FETCH (não em memória)?
- Cache Redis para leituras frequentes?
- Async/await sem `.Result` ou `.Wait()`?
- `AsNoTracking()` em leituras?

### 5. Anti-Patterns
- Evitou Generic Repository sobre EF Core?
- Evitou God classes (>300 linhas)?
- Evitou magic strings/numbers?

### 6. Conformidade com Specs
- Código segue as specs relevantes em `specs/`?
- Se a task veio de uma feature spec, todos os critérios de aceitação foram atendidos?

## Checklist Final
- [ ] Build passa (0 erros, 0 warnings)?
- [ ] Testes unitários passam?
- [ ] Testes de integração passam?
- [ ] Dados sensíveis protegidos?
- [ ] Validação implementada?
- [ ] Retornos RFC 9457?
- [ ] Specs seguidas?
