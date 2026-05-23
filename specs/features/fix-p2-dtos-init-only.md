# Fix: Request DTOs — Propriedades init-only e Expression-bodied (P2)

## Objetivo

Alterar propriedades de request DTOs de `{ get; set; }` para `{ get; init; }` e converter `MapToResponse` para expression-bodied.

## Regras de Negócio

1. Request DTOs: `{ get; set; }` → `{ get; init; }` em todos os arquivos
2. `ComicService.MapToResponse`: block body → expression-bodied member
3. `IComicTypeService.CreateAsync`: parâmetro `nome` → `name`
4. Magic string `"User"` → constante `Roles.User`

## Arquivos Afetados

- `src/FirstWebApi.Application/DTOs/Request/RegisterRequest.cs`
- `src/FirstWebApi.Application/DTOs/Request/LoginRequest.cs`
- `src/FirstWebApi.Application/DTOs/Request/RefreshTokenRequest.cs`
- `src/FirstWebApi.Application/DTOs/Request/FullProfileRequest.cs`
- `src/FirstWebApi.Application/DTOs/Request/ComicRequest.cs`
- `src/FirstWebApi.Application/DTOs/Request/ComicTypeRequest.cs`
- `src/FirstWebApi.Application/Services/ComicService.cs`
- `src/FirstWebApi.Application/Interfaces/IComicTypeService.cs`
- `src/FirstWebApi.Application/Services/ComicTypeService.cs`
- `src/FirstWebApi.Application/Services/AuthService.cs`
- `src/FirstWebApi.Application/Services/ProfileService.cs`

## Critérios de Aceitação

- [ ] Todos os request DTOs usam `init`
- [ ] `MapToResponse` é expression-bodied
- [ ] Parâmetro `nome` renomeado para `name` em ComicTypeService
- [ ] `"User"` substituído por constante em AuthService e ProfileService
- [ ] `dotnet build` passa
- [ ] Testes continuam passando
