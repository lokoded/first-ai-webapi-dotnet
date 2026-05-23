# Workflow SDD — FirstWebApi

Este documento define o fluxo de trabalho obrigatório para toda nova funcionalidade.

---

## Ciclo: Spec → Review → Branch → Implement → Test → PR → Archive

```
[Usuário] → pede funcionalidade
    ↓
[IA] → Fase 1: Spec
    ↓  cria specs/features/<nome>.md
[Usuário] → Fase 2: Review
    ↓  lê e aprova/ajusta a spec
[IA] → Fase 3: Branch + Implementação
    ↓  git checkout -b feature/<nome>, depois codifica
[IA] → Fase 4: Testes
    ↓  unit + integration (ou existentes)
[IA] → Fase 5: Verificação
    dotnet build && dotnet test
    ↓
[Usuário] → aprova resultado
    ↓
[IA] → Fase 5.5: Push + PR
    ↓  git push -u origin <branch> && gh pr create
[IA] → Fase 6: Archive
    ↓  move spec para specs/archive/<feature>/
[IA] → (após merge) deletar branch local + remota
```

---

## Fase 3 — Branch + Implementação

A IA cria a branch: `git checkout -b feature/<nome>` (ou `fix/<nome>`).
Depois implementa seguindo:
- `specs/00-architecture.md` — estrutura de camadas
- `specs/01-coding-standards.md` — convenções de código
- `specs/02-security-owasp.md` — segurança
- `specs/03-api-conventions.md` — HTTP, auto-validation, paginação
- `specs/04-ef-core-practices.md` — EF Core queries e migrations
- `specs/05-logging-serialization.md` — logs
- `specs/08-anti-patterns.md` — o que evitar

## Fase 5 — Verificação + PR

```powershell
dotnet build
dotnet test
```

Se falhar, corrigir. Se passar:
1. `git push -u origin <branch>`
2. `gh pr create --base develop`
3. Aguardar merge.

## Fase 6 — Archive

Após o merge na `develop`, a IA:
1. Deleta a branch local e remota (`git branch -d` + `git push origin --delete`).
2. Move spec de `specs/features/` para `specs/archive/<feature>/`.


---

## Regras de Ouro

1. **Spec primeiro, código depois** — sem exceção
2. **Spec vive em `specs/features/` até ser concluída** — versionada como código
3. **Spec aprovada = contrato** — implementação deve seguir a spec à risca
4. **Spec pode evoluir** — se durante implementação descobrir algo novo, atualizar a spec primeiro
5. **Toda spec tem critérios de aceitação** — sem checklist, não está pronta
6. **Feature concluída e mergeada → spec vai para `specs/archive/<feature>/`** — mantém `features/` limpo para specs ativas
