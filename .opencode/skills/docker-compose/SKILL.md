---
name: docker-compose
description: Use quando precisar gerenciar a infraestrutura Docker do projeto — subir, derrubar, verificar logs, rebuildar containers. NÃO usar para criar ou modificar código da aplicação.
---

## Infraestrutura Local

- **sqlserver** — SQL Server 2022
- **redis** — Redis (cache)
- **localstack** — AWS KMS simulado
- **api** — Aplicação .NET 10 (opcional via Docker)

## Comandos Principais

| Ação | Comando |
|------|---------|
| Subir infra | `docker compose up -d` |
| Derrubar (mantém dados) | `docker compose down` |
| Derrubar (destrói dados) | `docker compose down -v` |
| Ver status | `docker compose ps` |
| Ver logs de um serviço | `docker compose logs -f <service>` |
| Ver logs de todos | `docker compose logs -f` |
| Rebuild de um serviço | `docker compose build <service>` |
| Rebuild + subir | `docker compose up -d --build` |
| Restart de um serviço | `docker compose restart <service>` |

## Health Checks

```powershell
# SQL Server
docker compose exec sqlserver sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -Q "SELECT 1"

# Redis
docker compose exec redis redis-cli ping  # Deve retornar: PONG

# LocalStack
docker compose exec localstack awslocal kms list-keys

# API
curl http://localhost:5043/health
```

## Armadilhas

1. **`docker compose down -v` destrói VOLUMES** — Dados de banco, Redis e KMS são perdidos. Use sem `-v` para preservar.
2. **SQL Server leva 10-30s para iniciar** — API pode falhar se conectar antes.
3. **LocalStack v4+ exige auth token** — Configurar `LOCALSTACK_AUTH_TOKEN` no `.env`.
4. **Portas conflitantes** — SQL Server (1433) e Redis (6379) podem conflitar com serviços locais.
5. **`docker compose down` não para a API via `dotnet run`** — API rodando fora do Docker não é afetada.
