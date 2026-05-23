---
name: migration
description: Use quando precisar criar, aplicar, reverter ou gerenciar migrations do EF Core, scripts SQL, ou alterações no banco de dados. NÃO usar para CRUD de entidades ou endpoints.
---

## Workflow de Migrations

### 1. Criar

```powershell
dotnet ef migrations add <NomeDaMigration> `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

- Nome em inglês, PascalCase, descritivo (ex: `AddUserAuditFields`)
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

### 4. Reverter banco

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
- Nunca aplicar migration automática em produção

### 6. Script de rollback

```powershell
dotnet ef migrations script <MigrationDe> <MigrationPara> --idempotent -o scripts/rollback.sql `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

### 7. Listar

```powershell
dotnet ef migrations list `
  --project src/FirstWebApi.Infrastructure `
  --startup-project src/FirstWebApi.WebApi
```

## Regras

- Migration automática roda apenas em Development/Testing
- Em produção: **sempre** usar script idempotente
- Migration nova exige `DbSet` no `AppDbContext` e `IEntityTypeConfiguration`
- Se adicionar coluna NOT NULL em tabela com dados, usar valor padrão
- Nunca editar migrations já aplicadas — criar nova migration

## Armadilhas

- Docker precisa estar rodando para `database update`
- `dotnet ef migrations remove` só remove a ÚLTIMA migration não aplicada
- Conflito de merge em migrations: reverter para antes do fork, depois recriar
