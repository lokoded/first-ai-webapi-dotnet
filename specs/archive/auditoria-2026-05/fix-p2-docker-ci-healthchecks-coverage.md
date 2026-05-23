# Fix: Docker/CI — Healthchecks, Vulnerability Scan, Coverage Upload (P2)

## Objetivo

Adicionar healthchecks no docker-compose, vulnerability scan e upload de cobertura no CI.

## Regras de Negócio

1. Docker compose: healthcheck em sqlserver, redis (se mantido), localstack
2. CI: step `dotnet list package --vulnerable` após testes
3. CI: step `actions/upload-artifact` para relatórios de cobertura
4. CI: renomear step "Testes Unitários" → "Unit Tests"

## Arquivos Afetados

- `docker-compose.yml`
- `.github/workflows/ci.yml`

## Critérios de Aceitação

- [ ] Healthcheck presente em todos os serviços do docker-compose
- [ ] CI executa `dotnet list package --vulnerable`
- [ ] CI faz upload de relatórios de cobertura
- [ ] `dotnet build` passa
