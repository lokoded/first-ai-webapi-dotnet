# Diário de Aprendizado

> Fonte: AGENTS.md (extraído em 21/05/2026)

Registro de aprendizados, descobertas e lições durante o desenvolvimento.

---

### 20/05/2026 — Módulo 0 concluído

**Setup & Diagnóstico**
- Build 0 erros ✅, Unit tests 35/35 ✅, Integration tests 21/21 ✅
- Docker Compose com 4 containers rodando
- `.editorconfig` + `.vscode/` criados para padronização
- `.opencode/context.md` criado como baseline

**Descobertas**:
- Integration tests compartilham banco `FirstWebApiDb_Test` — rodar sempre em sequência
- KMS `GetAwaiter().GetResult()` no construtor trava se LocalStack estiver offline
- OpenCode tem agents nativos `build`, `explore`, `plan`, `general` — todos configuráveis

### 20/05/2026 — Módulo 1 concluído

**OpenCode, Agents & CLI**
- `opencode.jsonc` criado com schema oficial + agents + instructions
- `rules.md` com 45+ regras do projeto carregadas no contexto
- 3 skills no formato oficial `SKILL.md` (criar-endpoint, criar-entidade, code-review)
- 3 agents em `.opencode/agents/` (dev-agent, tester, security-reviewer)
- `.opencode/prompts.md` com templates e boas práticas
- Evolução do AGENTS.md com ADR e diário de aprendizado

**Descobertas**:
- OpenCode lê skills de `.opencode/skills/<nome>/SKILL.md` automaticamente
- Agentes em markdown com YAML frontmatter
- JSONC precisa de aspas nas chaves (`"key"`, não `key:`)

### 20/05/2026 — Módulo 2 concluído

**DevSecOps (OWASP Top 10)**
- Rate limiting nativo (.NET 10) — Auth: 10 req/min, Default: 100 req/min
- Refresh tokens com rotação (SHA256 no DB, 7 dias de validade)
- Security headers: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy
- Account lockout: 5 tentativas, 15 min de bloqueio
- CORS restrito por ambiente (via configuração)
- appsettings.json limpo (sem credenciais hardcoded)
- Migration `AddRefreshTokens` gerada
- 56/56 testes passando

**Descobertas**:
- `opencode.json` usa `"agent"` (singular) não `"agents"`
- Skills requerem SKILL.md com YAML frontmatter em subfolder
- EF Core 10 bloqueia `PendingModelChangesWarning` por padrão — precisa configurar em Dev/Testing

### 21/05/2026 — Módulo 3 concluído

**CI/CD com GitHub Actions**
- Repositório Git inicializado e enviado para GitHub (SSH)
- Pipeline CI com build + testes unitários + testes de integração
- SQL Server, Redis e LocalStack como service containers no CI
- Credenciais movidas para GitHub Secrets (segurança)
- Quality gate: cobertura mínima de 70% via coverlet + runsettings
- Badge de status do CI no README
- Workflow de varredura de vulnerabilidades (semanal + push)
- Branch `develop` criada como padrão de trabalho

**Descobertas**:
- `dotnet add package` resolve versão mais recente automaticamente (coverlet 6.0.4 → 10.0.1)
- SSH na porta 22 pode ser bloqueado por rede; alternativa é porta 443
- GitHub API retorna 404 para repositórios privados sem autenticação
- Service containers no GitHub Actions substituem o docker-compose local
