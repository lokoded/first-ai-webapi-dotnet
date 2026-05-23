# Fix: Domain ValueObjects — readonly record struct (P2)

## Objetivo

Alterar `Cpf` e `Email` de `record class` para `readonly record struct` conforme spec 01.

## Regras de Negócio

1. `Cpf`: mudar de `public record class Cpf` para `public readonly record struct Cpf`
2. `Email`: mudar de `public partial record class Email` para `public readonly partial record struct Email`
3. Manter toda validação e comportamento existente

## Arquivos Afetados

- `src/FirstWebApi.Domain/ValueObjects/Cpf.cs`
- `src/FirstWebApi.Domain/ValueObjects/Email.cs`

## Critérios de Aceitação

- [ ] Cpf é `readonly record struct`
- [ ] Email é `readonly partial record struct`
- [ ] `dotnet build` passa
- [ ] Testes existentes continuam passando
