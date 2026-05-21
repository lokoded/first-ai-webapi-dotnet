---
name: migration
description: Use quando precisar criar, aplicar, reverter ou gerenciar migrations do EF Core, scripts SQL, ou alterações no banco de dados. NÃO usar para CRUD de entidades ou endpoints.
---

## Workflow de Migrations

### 1. Criar uma nova migration

```powershell
dotnet ef migrations add <NomeDaMigration> `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

- `<NomeDaMigration>` em inglês, PascalCase, descritivo (ex: `AddUserAuditFields`)
- Sempre verificar o código gerado em `Data/Migrations/` antes de aplicar

### 2. Reverter (antes de commitar)

```powershell
dotnet ef migrations remove `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

Remove a última migration do código (não desfaz o banco).

### 3. Aplicar no banco local

```powershell
dotnet ef database update `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

**Pré-requisito**: Docker rodando (`docker compose up -d`).

### 4. Reverter banco para uma migration anterior

```powershell
dotnet ef database update <NomeDaMigrationAnterior> `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

### 5. Script idempotente para produção

```powershell
dotnet ef migrations script --idempotent -o scripts/migration.sql `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

- Revisar o SQL gerado via PR antes de aplicar
- Nunca aplicar migration automática em produção (`Migrate()` só roda em Development/Testing)

### 6. Script de rollback entre versões

```powershell
dotnet ef migrations script <MigrationDe> <MigrationPara> --idempotent -o scripts/rollback.sql `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

### 7. Listar migrations

```powershell
dotnet ef migrations list `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

## Regras

- Migration automática roda apenas em `Development`/`Testing` (via `Program.cs`)
- Em produção: **sempre** usar script idempotente
- Migration nova exige `DbSet` no `AppDbContext` e `IEntityTypeConfiguration` (classes separadas em `Data/Configurations/`)
- Se adicionar coluna em entidade existente, verificar se há dados no banco que podem quebrar com `NOT NULL`
- Nunca editar migrations já aplicadas — criar nova migration

## Armadilhas

- Docker precisa estar rodando para `database update`
- `dotnet ef migrations remove` só remove a ÚLTIMA migration não aplicada
- Se der conflito de merge em migrations, resolver manualmente: reverter para antes do fork, depois recriar
