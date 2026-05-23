# 01 — Convenções de Código

## Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Classes, métodos, propriedades | PascalCase | `UserService`, `GetByIdAsync` |
| Parâmetros, variáveis locais | camelCase | `userId`, `request` |
| Campos privados | `_camelCase` | `_context`, `_logger` |
| Interfaces | Prefixo `I` | `IUserRepository` |
| Métodos async | Sufixo `Async` | `SaveChangesAsync` |
| Tipos explícitos | Em assinaturas públicas | `public async Task<UserResponse> GetAsync(...)` |
| `var` | Só quando tipo é óbvio | `var user = new User()` (ok), `var result = GetData()` (evitar) |

## Expressões

- **File-scoped namespaces** (C# 10+): `namespace FirstWebApi.Domain.Entities;`
- **Expression-bodied members** quando for expressão única: `public int Id => _id;`
- **Primary constructors** (C# 12+): `public class UserService(IUserRepository _repo)`
- **Preferir string interpolation** sobre concatenação (exceto em logs estruturados)

## Boas Práticas

- Async/await para toda I/O. Nunca `.Result` ou `.Wait()`. `CancellationToken` como último parâmetro.
- Async desde o início — `ValueTask` só com justificativa de performance
- Imutabilidade em ValueObjects (`readonly record struct`)
- Propriedades init-only em DTOs de request quando possível
- Métodos com uma única responsabilidade (se precisa de "e" para descrever, divida)
- Nomes revelam intenção — sem abreviações ou genéricos

## Idiomas

- Português brasileiro: strings para o usuário, mensagens de validação
- Inglês: identificadores (classes, métodos, variáveis), comentários técnicos, commits
