# FirstWebApi — Contexto do Projeto

> Snapshot do projeto — atualizado em 20/05/2026 (Módulo 2 concluído).
> Atualize este arquivo conforme o projeto evolui.

---

## Estrutura

```
FirstWebApi.slnx
├── opencode.jsonc                 # Config OpenCode do projeto
├── AGENTS.md                      # Contexto persistente (memória da IA)
├── .editorconfig                  # Padronização de estilo
├── .vscode/                       # Settings + extensões recomendadas
├── .opencode/
│   ├── rules.md                   # Regras do projeto (carregado no contexto)
│   ├── context.md                 # Baseline do projeto
│   ├── prompts.md                 # Guia de prompts e boas práticas
│   ├── agents/                    # Agentes customizados (dev, tester, security)
│   ├── skills/                    # Skills reutilizáveis (criar-endpoint, criar-entidade, code-review)
├── src/
│   ├── FirstWebApi.Domain/        # Entities, ValueObjects, Enums, Repository interfaces
│   ├── FirstWebApi.Application/   # DTOs, Services, Validators
│   ├── FirstWebApi.Infrastructure/ # EF Core, Repositories, JWT, KMS, Redis decorators
│   ├── FirstWebApi.WebApi/        # Controllers, Middleware, Program.cs
└── tests/
    ├── FirstWebApi.UnitTests/     # 35 testes
    └── FirstWebApi.IntegrationTests/ # 21 testes
```

## Status Atual

| Item | Status |
|------|--------|
| Build | 0 erros, 0 warnings |
| Testes unitários | 35/35 ✅ |
| Testes de integração | 21/21 ✅ |
| Docker Compose | sqlserver + redis + localstack + api rodando |
| GitHub Actions | ✅ CI + Security Scan |
| Cobertura de código | ❌ Não mensurada |
| OpenCode | v1.15.5 configurado |
| Migrations | 2 (InitialCreate, AddRefreshTokens) |

## Funcionalidades Implementadas

- [x] CRUD de HQs (por usuário)
- [x] CRUD de tipos de HQ (Admin)
- [x] Cadastro/login com JWT
- [x] Criptografia LGPD (CPF, RG, Endereço) via KMS
- [x] Cache Redis via decorators
- [x] Paginação (em memória — precisa migrar para server-side)
- [x] Health check básico
- [x] Exception middleware com RFC 9457
- [x] Logging em arquivo com TraceId
- [x] FluentValidation em todos os inputs
- [x] Rate limiting (Auth: 10 req/min, Default: 100 req/min)
- [x] Refresh tokens com rotação e revogação
- [x] Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- [x] Account lockout (5 tentativas, 15 min)
- [x] CORS por ambiente
- [x] appsettings.json sem credenciais hardcoded

## Gap Analysis (identificados para evolução)

1. Paginação server-side (OFFSET/FETCH)
2. CI/CD (GitHub Actions)
3. Cobertura de testes mensurada
4. Health checks com probes reais (DB, Redis, KMS)
5. Serilog ou evolução do FileLogger
6. Endpoints faltantes (PUT address, DELETE user)
7. API versioning
8. Audit trail
9. Testes de mutation e segurança

## Plano de Estudos

| Módulo | Tema | Status |
|--------|------|--------|
| 0 | Setup & Diagnóstico | ✅ Concluído |
| 1 | OpenCode, Agents & CLI | ✅ Concluído |
| 2 | DevSecOps (OWASP Top 10) | ✅ Concluído |
| 3 | CI/CD (GitHub Actions) | ✅ Concluído |
| 4 | Performance & Arquitetura | ⬜ |
| 5 | Features Corporativas | ⬜ |
| 6 | Testes Avançados | ⬜ |
| 7 | Design Patterns | ⬜ |
| 8 | IA Avançada | ⬜ |

## Comandos Rápidos

```powershell
dotnet build                                  # Compilar
dotnet test tests/FirstWebApi.UnitTests       # Testes unitários
dotnet test tests/FirstWebApi.IntegrationTests # Testes de integração
docker compose up -d                          # Subir infra
docker compose down -v                        # Destruir infra
```
