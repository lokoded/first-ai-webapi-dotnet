# Feature: Auditoria Geral do Projeto

## Objetivo

Realizar auditoria completa de todo o repositório FirstWebApi contra todas as specs definidas em `specs/`, identificando bugs, violações de arquitetura, problemas de segurança, código fora dos padrões, inconsistências e resquícios de Redis. Entregar relatório priorizado com ordem de correção sugerida.

## Escopo

Todos os artefatos do repositório:
- Código-fonte (Domain, Application, Infrastructure, WebApi)
- Testes (UnitTests, IntegrationTests)
- Docker (Dockerfile, docker-compose)
- CI/CD (GitHub Actions workflows)
- Scripts e configurações de infraestrutura
- Resquícios de Redis (cache, configurações, dependências)

## Referências de Conformidade

- `specs/00-architecture.md` — Clean Architecture, dependências, RFC 9457
- `specs/01-coding-standards.md` — nomenclatura, expressões, boas práticas
- `specs/02-security-owasp.md` — OWASP A01-A10, LGPD, JWT, auth
- `specs/03-api-conventions.md` — auto-validation, HTTP codes, paginação, `[ProducesResponseType]`
- `specs/04-ef-core-practices.md` — AsNoTracking, Includes, queries, migrações
- `specs/05-logging-serialization.md` — logs estruturados, o que nunca logar
- `specs/06-docker-ci.md` — Docker, CI/CD, GitHub Actions
- `specs/07-testing-strategy.md` — naming AAA, cobertura de cenários
- `specs/08-anti-patterns.md` — o que evitar

## Critérios de Prioridade

| Prioridade | Critério |
|------------|----------|
| **P0** | Bug em produção, falha de segurança, dado sensível exposto, dado não cifrado |
| **P1** | Violação de arquitetura, código quebrado, comportamento incorreto, query ineficiente |
| **P2** | Estilo, nomenclatura, boas práticas, documentação, código morto |

## Pós-Auditoria

Cada problema identificado vira uma spec/issue separada seguindo o workflow SDD.

## Critérios de Aceitação

- [ ] Domain verificado contra specs 00, 01, 08
- [ ] Application verificado contra specs 00, 01, 03, 08
- [ ] Infrastructure verificado contra specs 02, 04, 05, 08
- [ ] WebApi verificado contra specs 00, 01, 03, 08
- [ ] Docker/CI verificado contra spec 06
- [ ] Testes verificados contra spec 07
- [ ] Resquícios de Redis identificados
- [ ] Relatório priorizado entregue em `specs/features/auditoria-geral-relatorio.md`
- [ ] Ordem de correção sugerida

## Formato do Relatório

Cada entrada no relatório segue:

```md
## [P0/P1/P2] Título do problema
- **Arquivo**: `src/.../Arquivo.cs:42`
- **Spec violada**: `02-security-owasp.md`
- **Problema**: descrição objetiva
- **Sugestão**: o que fazer para corrigir
```
