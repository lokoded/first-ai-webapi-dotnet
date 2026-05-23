# Fix: Refresh Token em HttpOnly Cookie (P0)

## Objetivo

Mover refresh token do JSON body para cookie HttpOnly+Secure+SameSite=Strict, conforme especificação de segurança.

## User Story

Como usuário autenticado, quero que meu refresh token seja armazenado em cookie seguro (não acessível via JavaScript) para mitigar ataques XSS.

## Regras de Negócio

1. Refresh token deve ser retornado em cookie HttpOnly, não no JSON body
2. Cookie: HttpOnly=true, Secure=true, SameSite=Strict
3. Endpoint `/refresh` deve ler o cookie, não o body
4. `AuthResponse` não deve mais conter `RefreshToken`
5. `RefreshTokenRequest` pode ser removido ou simplificado

## Arquivos Afetados

- `src/FirstWebApi.WebApi/Controllers/AuthController.cs`
- `src/FirstWebApi.Application/DTOs/Response/AuthResponse.cs`
- `src/FirstWebApi.Application/DTOs/Request/RefreshTokenRequest.cs`

## Critérios de Aceitação

- [x] Login seta cookie HttpOnly com refresh token
- [x] Refresh lê cookie em vez de body
- [x] `AuthResponse.RefreshToken` com `[JsonIgnore]`
- [x] `RefreshTokenRequest` removido (arquivo + validator)
- [x] `dotnet build` — 0 warnings, 0 errors
- [x] Testes de integração de auth — 9/10 passam (1 pré-existente)
