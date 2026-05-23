# Fix: Infrastructure — JWT Validation, FileLogger, Domain XML Doc (P2)

## Objetivo

Corrigir issues de infrastructure: validação de tamanho mínimo do JWT SecretKey, redação de dados sensíveis no FileLogger, e remover referência a EF Core no XML doc do Domain.

## Regras de Negócio

1. `TokenService`: validar que `secretKey.Length >= 32` (HMAC-SHA256 mínimo 256 bits)
2. `FileLogger`: aplicar redação de dados sensíveis também na mensagem formatada (não apenas nos campos KV)
3. `IComicRepository`: remover menção a `.Include(c => c.ComicType)` do XML doc

## Arquivos Afetados

- `src/FirstWebApi.Infrastructure/Services/TokenService.cs`
- `src/FirstWebApi.Infrastructure/Logging/FileLogger.cs`
- `src/FirstWebApi.Domain/Interfaces/IComicRepository.cs`

## Critérios de Aceitação

- [ ] TokenService lança exceção se SecretKey < 32 chars
- [ ] FileLogger redige dados sensíveis na mensagem formatada
- [ ] XML doc de IComicRepository não menciona EF Core
- [ ] `dotnet build` passa
- [ ] Testes continuam passando
