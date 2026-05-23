# Fix: Adicionar CancellationToken em Toda Cadeia (P2)

## Objetivo

Adicionar `CancellationToken` como último parâmetro em todas as interfaces de serviço, implementações, repositórios do Domain e controllers.

## Regras de Negócio

1. Adicionar `CancellationToken cancellationToken = default` como último parâmetro em:
   - Todas as interfaces `I*Service` na Application
   - Todas as implementações de serviço
   - Todas as interfaces `I*Repository` no Domain
   - Todas as implementações de repositório na Infrastructure
2. Propagar token do `HttpContext.RequestAborted` nos controllers
3. Passar token para chamadas EF Core (`SaveChangesAsync(cancellationToken)`, `ToListAsync(cancellationToken)`, etc.)

## Arquivos Afetados

- `src/FirstWebApi.Domain/Interfaces/I*.cs` (6 interfaces de repositório)
- `src/FirstWebApi.Application/Interfaces/I*Service.cs` (7 interfaces)
- `src/FirstWebApi.Application/Services/*.cs` (5 services)
- `src/FirstWebApi.Infrastructure/Repositories/*.cs` (5 repositories)
- `src/FirstWebApi.WebApi/Controllers/*.cs` (5 controllers)

## Critérios de Aceitação

- [ ] Todas as interfaces de serviço têm `CancellationToken` como último parâmetro
- [ ] Todas as implementações propagam o token
- [ ] Repositórios aceitam e usam o token
- [ ] Controllers passam `HttpContext.RequestAborted`
- [ ] `dotnet build` passa
- [ ] Testes existentes continuam passando
