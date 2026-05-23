# Fix: WebApi — Autorização, ProducesResponseType, Location Headers, MaskingHelper (P2)

## Objetivo

Corrigir convenções da WebApi: adicionar `[Authorize]` no AuthController, `[ProducesResponseType]` em endpoints sem documentação, corrigir Location headers, e mover MaskingHelper do controller para Application.

## Regras de Negócio

1. AuthController: adicionar `[Authorize]` na classe e `[AllowAnonymous]` em Register/Login/Refresh
2. AuthController e UsersController: adicionar `[ProducesResponseType]` em todos os endpoints
3. AuthController.Register: Location header com `CreatedAtAction` para UsersController.GetProfile
4. AdminComicTypesController.Create: Location header com `CreatedAtAction(nameof(GetById), ...)`
5. UsersController: mover `MaskingHelper.ApplyMask` para ProfileService

## Arquivos Afetados

- `src/FirstWebApi.WebApi/Controllers/AuthController.cs`
- `src/FirstWebApi.WebApi/Controllers/UsersController.cs`
- `src/FirstWebApi.WebApi/Controllers/AdminComicTypesController.cs`
- `src/FirstWebApi.Application/Services/ProfileService.cs`

## Critérios de Aceitação

- [ ] AuthController tem `[Authorize]` na classe
- [ ] Endpoints públicos têm `[AllowAnonymous]`
- [ ] Todos os endpoints de Auth e Users têm `[ProducesResponseType]`
- [ ] Location headers corretos (Register → GetProfile, Create → GetById)
- [ ] MaskingHelper não é chamado no controller
- [ ] `dotnet build` passa
- [ ] Testes continuam passando
