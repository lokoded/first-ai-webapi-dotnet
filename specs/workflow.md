# Workflow SDD — FirstWebApi

Este documento define o fluxo de trabalho obrigatório para toda nova funcionalidade.

---

## Ciclo: Spec → Review → Implement → Test

```
[Usuário] → pede funcionalidade
    ↓
[IA] → Fase 1: Spec
    ↓  cria specs/features/<nome>.md
[Usuário] → Fase 2: Review
    ↓  lê e aprova/ajusta a spec
[IA] → Fase 3: Implementação
    ↓  codifica seguindo specs/00-08
[IA] → Fase 4: Testes
    ↓  unit + integration (ou existentes)
[IA] → Fase 5: Verificação
    dotnet build && dotnet test
    ↓
[Usuário] → aprova resultado
    ↓
[IA] → commit (se aplicável) + pergunta se faz push
```

---

## Fase 1 — Spec

A IA cria um arquivo em `specs/features/<nome-da-feature>.md` usando o template.

### Conteúdo obrigatório:
1. **Objetivo** — o que esta feature faz (1 parágrafo)
2. **User Story** — "Como [ator], quero [ação] para [benefício]"
3. **Contrato da API** — método, rota, request (body/params), response, status codes
4. **Regras de negócio** — validações, autorização, ownership
5. **Exemplo de Request/Response**
6. **Cenários de Teste** — happy path + edge cases (tabela)
7. **Critérios de Aceitação** — checklist do que é "pronto"

## Fase 2 — Review

Usuário lê a spec, sugere ajustes, aprova ou solicita alterações.

Nenhum código é escrito antes da aprovação da spec.

## Fase 3 — Implementação

A IA implementa seguindo:
- `specs/00-architecture.md` — estrutura de camadas
- `specs/01-coding-standards.md` — convenções de código
- `specs/02-security-owasp.md` — segurança
- `specs/03-api-conventions.md` — HTTP, auto-validation, paginação
- `specs/04-ef-core-practices.md` — EF Core queries e migrations
- `specs/05-logging-serialization.md` — logs
- `specs/08-anti-patterns.md` — o que evitar

## Fase 4 — Testes

A IA escreve ou atualiza:
- Testes unitários (Moq) para o service
- Testes de integração (WebApplicationFactory) para o endpoint
- Cobertura dos status codes da spec

## Fase 5 — Verificação

```powershell
dotnet build
dotnet test
```

Se falhar, corrigir. Se passar, perguntar ao usuário se deseja commit/push.

---

## Regras de Ouro

1. **Spec primeiro, código depois** — sem exceção
2. **Spec vive em `specs/features/`** — versionada como código
3. **Spec aprovada = contrato** — implementação deve seguir a spec à risca
4. **Spec pode evoluir** — se durante implementação descobrir algo novo, atualizar a spec primeiro
5. **Toda spec tem critérios de aceitação** — sem checklist, não está pronta
