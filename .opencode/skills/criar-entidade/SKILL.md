---
name: criar-entidade
description: Use quando precisar criar, adicionar ou implementar uma nova entidade, modelo, tabela ou CRUD completo (Domain → Application → Infrastructure → WebApi → Testes). NÃO usar para endpoints avulsos.
---

## Passo 0 — Verificar Specs

Antes de começar, ler:
- `specs/00-architecture.md` — estrutura de camadas, restrições
- `specs/08-anti-patterns.md` — o que evitar

## Ordem de Implementação

### 1. Domain
- Entity em `Entities/` com construtor protegido (EF) + público
- Value Objects se aplicável (ex: Cpf, Email)
- Interface de repository em `Interfaces/`

### 2. Application
- `DTOs/Request/NomeRequest.cs` e `DTOs/Response/NomeResponse.cs`
- `Validators/NomeRequestValidator.cs`
- `Interfaces/INomeService.cs` + `Services/NomeService.cs`

### 3. Infrastructure
- `Data/Configurations/NomeConfiguration.cs` (Fluent API)
- `DbSet<Nome>` no `AppDbContext`
- `Repositories/NomeRepository.cs`
- Se for cache: `Repositories/Decorators/CachedNomeRepository.cs`
- Registrar DI em `Program.cs`

### 4. WebApi
- Controller seguindo `criar-endpoint` skill
- `[Authorize]` se aplicável

### 5. Testes
- Unit: Moq para repositórios, testar CRUD completo
- Integration: WebApplicationFactory, fluxo real com banco

## Referências

- `specs/04-ef-core-practices.md` — configurações, queries, migrações
- `specs/03-api-conventions.md` — contratos HTTP
- `specs/07-testing-strategy.md` — padrões de teste
