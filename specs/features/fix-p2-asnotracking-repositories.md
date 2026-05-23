# Fix: Adicionar AsNoTracking em Repositórios (P2)

## Objetivo

Adicionar `.AsNoTracking()` em todas as consultas de leitura nos repositórios que atualmente não o usam.

## Regras de Negócio

1. Consultas de leitura (GET) devem usar `.AsNoTracking()` para evitar tracking desnecessário
2. Consultas de escrita (CREATE/UPDATE/DELETE) não devem usar

## Arquivos Afetados

- `src/FirstWebApi.Infrastructure/Repositories/UserRepository.cs` (linhas 13, 18)
- `src/FirstWebApi.Infrastructure/Repositories/ComicRepository.cs` (linha 31)
- `src/FirstWebApi.Infrastructure/Repositories/ComicTypeRepository.cs` (linhas 13-15)
- `src/FirstWebApi.Infrastructure/Repositories/AddressRepository.cs` (linha 13)
- `src/FirstWebApi.Infrastructure/Repositories/RefreshTokenRepository.cs` (linhas 13-14, 19-21)

## Critérios de Aceitação

- [ ] UserRepository.GetByIdAsync com AsNoTracking
- [ ] UserRepository.GetByEmailAsync com AsNoTracking
- [ ] ComicRepository.GetByIdAsync com AsNoTracking
- [ ] ComicTypeRepository.GetAllAsync com AsNoTracking + comentário sobre lookup
- [ ] AddressRepository.GetByUserIdAsync com AsNoTracking
- [ ] RefreshTokenRepository.GetByTokenHashAsync com AsNoTracking
- [ ] RefreshTokenRepository.GetActiveByUserIdAsync com AsNoTracking
- [ ] `dotnet build` passa
- [ ] Testes continuam passando
