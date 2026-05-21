# Contexto Persistente do Projeto

Este documento é a fonte principal de contexto do projeto.
Nunca ignore, descarte, sobrescreva ou resuma excessivamente este contexto durante o desenvolvimento.
Todas as decisões técnicas, implementações e análises devem permanecer alinhadas às diretrizes definidas aqui.

---

# Stack Resumida

```
.NET 10 | EF Core + SQL Server | Redis | JWT | KMS | FluentValidation | xUnit + Moq
```

## Status Atual

- Build: ✅ 0 erros, 0 warnings
- Testes unitários: ✅ 35/35
- Testes de integração: ✅ 21/21 (Docker Compose rodando)
- GitHub Actions: ✅ CI + Security Scan
- Módulo atual: 3 (CI/CD)

---

# Filosofia do Projeto

Abordagem enxuta, objetiva, moderna, profissional, sustentável, segura e pragmática.

**Priorizar**: boas práticas modernas, simplicidade sustentável, arquitetura clara, código limpo, legibilidade, manutenção, organização, previsibilidade, segurança desde o início, decisões alinhadas ao mercado atual.

**Evitar**: overengineering, complexidade desnecessária, padrões enterprise sem necessidade, abstrações prematuras, arquitetura excessivamente acadêmica.

## Critérios de Decisão Técnica

1. Clareza → 2. Simplicidade → 3. Manutenibilidade → 4. Segurança → 5. Performance → 6. Escalabilidade → 7. Complexidade adicional só com justificativa.

**Evitar**: abstrações prematuras, arquitetura especulativa, dependências desnecessárias, otimizações sem necessidade real, padrões complexos sem ganho claro.

## Restrições Arquiteturais

Não usar (salvo necessidade clara): microservices, event sourcing, kubernetes, mensageria complexa, CQRS completo, DDD estratégico avançado, múltiplos bancos sem justificativa, abstrações genéricas excessivas.

Arquitetura: monolítica modular simples.

---

# Contexto Geral do Projeto

Este projeto é simultaneamente:
- ambiente de estudo;
- laboratório prático;
- projeto de evolução profissional;
- treinamento para mercado moderno .NET;
- ambiente de aprendizado de DevOps, segurança e IA aplicada ao desenvolvimento.

Priorizar: clareza, objetividade, qualidade profissional, simplicidade sustentável, aprendizado progressivo, exemplos reais, boas práticas modernas, decisões alinhadas ao mercado atual.

---

# Mapa de Contexto

Conteúdo especializado foi extraído para `context/`. Carregue o arquivo relevante conforme a tarefa.

| Arquivo | Conteúdo | Útil para |
|---------|----------|-----------|
| `context/philosophy.md` | Objetivo Principal, Filosofia, Critérios, Escopo, Restrições, Diretriz de Explicações, Padrão de Respostas, Evolução Progressiva | Todos os agentes |
| `context/architecture.md` | Objetivo Técnico, Organização da Solution, Requisitos Técnicos, Estrutura de Pastas, RFC 9457, ADRs (001-006) | dev-agent |
| `context/security.md` | OWASP Top 10, KMS/LGPD, Rate Limiting, Refresh Tokens, Security Headers, Account Lockout, CORS | security-reviewer |
| `context/devops.md` | Git Flow, CI/CD (GitHub Actions), LocalStack/AWS, Docker | dev-agent |
| `context/ai-workflows.md` | Aprendizado com IA, OpenCode, Agents, Skills, MCP, Prompts, Boas Práticas | Todos (quando aplicável) |
| `context/aprendizado.md` | Diário de Aprendizado (Módulos 0-3) | Todos (consulta histórica) |
| `context/reference.md` | Comandos, API Endpoints, Peculiaridades e Armadilhas, CRUD guide, Convenções de Teste | dev-agent, tester |
