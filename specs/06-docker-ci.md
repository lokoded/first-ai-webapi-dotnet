# 06 — Docker e CI/CD

## Docker

### Multi-stage build

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY .csproj → restore → COPY source → dotnet publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
COPY --from=build /app/publish .
USER appuser
HEALTHCHECK ...
```

### Regras
- Copiar `.csproj` + `dotnet restore` ANTES do código fonte (cache de layers)
- Nunca rodar como root (`USER appuser`)
- `.dockerignore` excluindo `bin/`, `obj/`, `.git/`, `node_modules/`
- Healthcheck em todos os serviços no docker-compose
- Nunca expor portas de banco em produção
- Secrets via `.env` (dev) ou secrets manager (prod)

### Docker Compose

Infraestrutura local:
- **sqlserver** — SQL Server 2022
- **redis** — Cache
- **localstack** — AWS KMS simulado
- **api** — Aplicação .NET 10 (opcional via Docker)

## CI/CD (GitHub Actions)

### Pipeline de CI
- Trigger: push para `develop`, `main`, `feature/*`, `fix/*`
- Passos:
  1. Checkout
  2. Setup .NET 10
  3. `dotnet restore`
  4. `dotnet build`
  5. `dotnet test` (unitários)
  6. `dotnet list package --vulnerable`
  7. Cobertura de código (coverlet) — threshold mínimo 70% por projeto, falha se abaixo

### Pipeline de Security Scan
- Trigger: push para `develop` e `main`
- Escaneamento de dependências vulneráveis
- Escaneamento de secrets no código

## Git Flow

- `feature/*` e `fix/*` → partem de `develop`, PR para `develop`
- `hotfix/*` → parte de `main`, PR para `main`, depois merge back para `develop`
- `release/*` → parte de `develop`, PR para `main`
- Branches merged são deletadas local e remotamente
- `main` só recebe merge de `develop` ou `hotfix/*`
