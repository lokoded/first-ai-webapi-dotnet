# Fix: Remover Resquícios de Redis Não Utilizados (P1)

## Objetivo

Remover configurações de Redis que não são consumidas por código algum: connection string, serviço docker-compose, service no CI.

## Regras de Negócio

1. Remover `"Redis"` de `ConnectionStrings` em `appsettings.json`
2. Remover serviço `redis` do `docker-compose.yml` (container, env vars, depends_on, volume)
3. Remover serviço `redis` do CI (`ci.yml`)
4. Remover env `ConnectionStrings__Redis` do CI
5. Atualizar comentário obsoleto sobre StackExchange.Redis

## Arquivos Afetados

- `src/FirstWebApi.WebApi/appsettings.json`
- `docker-compose.yml`
- `.github/workflows/ci.yml`

## Critérios de Aceitação

- [ ] Connection string Redis removida de appsettings.json
- [ ] Serviço redis removido do docker-compose.yml
- [ ] Volume redis_data removido
- [ ] Service redis removido do CI
- [ ] Env `ConnectionStrings__Redis` removida do CI
- [ ] `dotnet build` passa
