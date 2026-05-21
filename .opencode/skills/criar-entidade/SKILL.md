---
name: criar-entidade
description: Instruções para adicionar uma nova entidade com CRUD completo na FirstWebApi
---

## Ordem de Implementação

### 1. Domain
- Entity em `Entities/` com construtor protegido (EF) + público
- Interface em `Interfaces/`

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
- Controller com `[Authorize]` se aplicável
- Seguir a skill `criar-endpoint`

### 5. Testes
- Unit: Moq para repositórios, testar CRUD completo
- Integration: WebApplicationFactory, fluxo real com banco
