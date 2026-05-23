# Fix: Dockerfile — Adicionar USER appuser (P1)

## Objetivo

Adicionar `USER appuser` no estágio runtime do Dockerfile para que o container não rode como root.

## Regras de Negócio

1. Adicionar `USER appuser` após `WORKDIR /app` no estágio runtime
2. Imagens `aspnet` já criam o usuário `appuser` automaticamente
3. Healthcheck deve vir depois da definição do usuário

## Arquivo Afetado

- `Dockerfile`

## Critérios de Aceitação

- [ ] `USER appuser` presente no Dockerfile após WORKDIR /app
- [ ] Container não roda como root
- [ ] Healthcheck continua funcional
- [ ] `dotnet build` passa
