# Workflow SDD — FirstWebApi

Este documento define o fluxo de trabalho obrigatório para toda nova funcionalidade.

---

## Ciclo: Spec → Review → Branch → Implement → Test → PR → Archive

```
[Usuário] → solicita funcionalidade

[IA] → Fase 1: Spec
    ↓ cria specs/features/<nome>.md

[Usuário] → Fase 2: Review
    ↓ aprova ou ajusta a spec

[IA] → Fase 3: Branch
    ↓ cria branch a partir de develop

[IA] → Fase 4: Implementação
    ↓ codifica seguindo specs/00-08

[IA] → Fase 5: Testes
    ↓ unit + integration tests

[IA] → Fase 6: Verificação
    ↓ dotnet build && dotnet test

[Usuário] → aprova resultado

[IA] → Fase 7: Commit + Push + PR
    ↓ cria PR para develop

[Usuário] → review + merge

[IA] → Fase 8: Archive
    ↓ move spec concluída para archive specs/archive/<feature>/
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

## Fase 3 — Branch

A IA cria uma branch a partir de `develop`.

### Padrões de branch
```text
feature/<nome>
fix/<nome>
refactor/<nome>
chore/<nome>
```

### Exemplo
```bash
git checkout develop
git pull
git checkout -b feature/user-authentication
```


## Fase 4 — Implementação

A IA implementa seguindo:
- `specs/00-architecture.md` — estrutura de camadas
- `specs/01-coding-standards.md` — convenções de código
- `specs/02-security-owasp.md` — segurança
- `specs/03-api-conventions.md` — HTTP, auto-validation, paginação
- `specs/04-ef-core-practices.md` — EF Core queries e migrations
- `specs/05-logging-serialization.md` — logs
- `specs/08-anti-patterns.md` — o que evitar

### Regras durante implementação
- não alterar contrato aprovado sem atualizar a spec
- manter consistência arquitetural
- evitar abstrações desnecessárias
- seguir padrões existentes do projeto
- atualizar a spec primeiro caso o escopo evolua


## Fase 5 — Testes

A IA escreve ou atualiza:
- Testes unitários (Moq)
- Testes de integração (WebApplicationFactory)
- cenários da spec
- Cobertura/validações de status codes da spec


## Fase 6 — Verificação
```powershell
dotnet build
dotnet test
```
- corrigir
- reexecutar validações

Nenhum PR deve ser criado com build/test quebrado.


## Fase 7 — Commit + Push + PR

Após aprovação do usuário:
```bash
git add .
git commit -m "<comment>"
git push -u origin <branch>
gh pr create --base develop
```
### Regras

- PR sempre para `develop`
- commits devem seguir Conventional Commits
- branch deve permanecer sincronizada com `develop`


## Fase 8 — Archive

Após o merge na `develop`, a IA:
1. Deleta a branch local e remota (`git branch -d` + `git push origin --delete`).
2. Mover specs de concluídas de `specs/features/` para `specs/archive/<nome-da-feature>/`.
3. Exceção: specs de contrato de API pública de longo prazo podem permanecer em `features/`.
   


---

## Regras de Ouro

1. **Spec primeiro, código depois** — sem exceção
2. **Spec vive em `specs/features/` até ser concluída** — versionada como código
3. **Spec aprovada = contrato** — implementação deve seguir a spec à risca
4. **Spec pode evoluir** — se durante implementação descobrir algo novo, atualizar a spec primeiro
5. **Toda spec tem critérios de aceitação** — sem checklist, não está pronta
6. **Feature concluída e mergeada → spec vai para `specs/archive/<feature>/`** — mantém `features/` limpo para specs ativas
