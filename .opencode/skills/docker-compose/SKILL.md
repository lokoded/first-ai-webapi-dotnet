---
name: docker-compose
description: Use quando precisar gerenciar a infraestrutura Docker do projeto — subir, derrubar, verificar logs, rebuildar containers. NÃO usar para criar ou modificar código da aplicação.
---

## Gerenciamento da Infraestrutura

A infraestrutura local é composta por:
- **sqlserver** — SQL Server 2022
- **redis** — Redis (cache)
- **localstack** — AWS KMS simulado
- **api** — A aplicação .NET 10 (opcional via Docker)

### Comandos principais

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

### Health checks

**SQL Server** (testar se está aceitando conexões):
```powershell
docker compose exec sqlserver sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -Q "SELECT 1"
```

**Redis**:
```powershell
docker compose exec redis redis-cli ping
# Deve retornar: PONG
```

**LocalStack**:
```powershell
docker compose exec localstack awslocal kms list-keys
```

**API** (health endpoint):
```powershell
curl http://localhost:5043/health
```

### Armadilhas

1. **`docker compose down -v` destrói VOLUMES** — Dados de banco, Redis e KMS são perdidos. Use sem `-v` se quiser preservar dados
2. **SQL Server leva 10-30s para iniciar** — A API pode falhar se conectar antes do banco estar pronto
3. **LocalStack v4+ exige auth token** — Configurar `LOCALSTACK_AUTH_TOKEN` no `.env` (template disponível)
4. **Portas conflitantes** — Se você já tem SQL Server ou Redis rodando localmente, os containers podem falhar por conflito de porta (1433, 6379)
5. **`docker compose down` não para a API se ela foi iniciada via `dotnet run`** — A API via `dotnet run` roda fora do Docker
