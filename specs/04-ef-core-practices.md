# 04 — Práticas de EF Core

## Configuração

- `AppDbContext` extends `IdentityDbContext<User, IdentityRole<Guid>, Guid>`
- Configurações Fluent API em classes separadas em `Data/Configurations/` (uma por entidade)
- `DbSet<T>` para cada entidade no `AppDbContext`
- DbContext registrado como Scoped

## Queries

- `AsNoTracking()` em todas as leituras (entidades apenas para consulta)
- `Include()` explícito — nunca lazy loading
- `Select()` apenas as colunas necessárias (evitar SELECT *)
- Sempre paginar com `Skip()`/`Take()` — nunca `ToList()` sem paginação
- `Where()` antes de `ToList()` (filtrar no banco, não em memória)
- Bulk operations: `ExecuteUpdateAsync`/`ExecuteDeleteAsync` quando aplicável

## Migrations

- Nome em inglês, PascalCase, descritivo: `AddUserAuditFields`
- Verificar código gerado antes de aplicar
- Nunca editar migrations já aplicadas — criar nova migration
- Em produção: script idempotente (`dotnet ef migrations script --idempotent`)
- Migration automática só roda em Development/Testing

## Performance

- Índices em foreign keys e colunas de filtro frequentes
- Paginação com OFFSET/FETCH (não em memória)
- Cache Redis via decorators para leituras frequentes (cache-aside)
- `Count()` separado para total de páginas (não carregar dados só para contar)

## ValueConverters

- `DadoProtegido` usa ValueConverter personalizado na Infrastructure
- Domain nunca conhece formato de serialização dos dados protegidos
- Trocar algoritmo criptográfico = alterar apenas o ValueConverter + serviço

## Regras

- `MigrateAsync()` só em Development/Testing. Em produção: script SQL
- Rollback: `dotnet ef migrations script <De> <Para> --idempotent`
- Conflito de merge em migrations: reverter para antes do fork, depois recriar
- Adicionar coluna NOT NULL em tabela com dados existentes exige valor padrão ou migration em duas etapas
