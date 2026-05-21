# Prompts e Boas Práticas — FirstWebApi

> Guia de como usar a IA de forma eficiente no projeto.
> Foco em: reduzir tokens, melhorar previsibilidade, evitar alucinações.

---

## 1. Estrutura de Prompt Eficiente

Para obter respostas consistentes, use esta estrutura:

```
[Contexto] O que você precisa saber sobre o projeto
[Tarefa]   O que exatamente deve ser feito
[Regras]   Restrições e convenções a seguir
[Exemplo]  (opcional) Como algo similar foi feito antes
```

**Exemplo prático:**

```
Estamos na FirstWebApi, um projeto .NET 10 em Clean Architecture.
Preciso criar um endpoint PUT /api/comics/{id}/archive para arquivar uma HQ.
Regras: usar FluentValidation, retornar NoContent(), validar ownership do usuário.
Veja como o PUT /api/comics/{id} foi implementado em ComicsController.cs como referência.
```

## 2. Use Skills e Agents

| Quando | Use |
|--------|-----|
| Criar um endpoint novo | `@dev-agent` + skill `criar-endpoint` |
| Adicionar entidade CRUD | skill `criar-entidade` |
| Revisar código | `@security-reviewer` ou skill `code-review` |
| Escrever testes | `@tester` |

## 3. Reduza Consumo de Tokens

- **Não peça análise de código que já funciona** — foco no que mudou
- **Prefira referências a arquivos** (`@caminho/arquivo.cs`) em vez de copiar código
- **Seja específico** — "crie um endpoint GET" gera mais iterações que "crie GET /api/comics/{id}/history retornando ComicHistoryResponse"
- **Evite contexto desnecessário** — não inclua specs inteiras se o relevante é 3 linhas
- **Use `@agent` para tarefas paralelas** — um agente pesquisa enquanto outro implementa

## 4. Evite Alucinações

- Peça referências: "baseado em como ComicsController implementa o GET"
- Peça confirmação: "antes de criar, leia o `Program.cs` para ver como o DI está configurado"
- Para código novo, peça primeiro um plano (use o Plan agent com Tab)
- Desconfie de bibliotecas que a IA sugere — verifique se já estão no `.csproj`

## 5. Prompts Reutilizáveis (Templates)

### Template: Criar endpoint simples
```
Crie um endpoint [METHOD] /api/[rota] no [ControllerName].
- Request: [NomeRequest]
- Response: [NomeResponse]
- Auth: [sim/não/admin]
- Regras: [regras de negócio]
Use o padrão dos outros controllers como referência.
```

### Template: Code review
```
@security-reviewer
Revise o código em @[arquivo] contra OWASP Top 10.
Foco em: [auth/validação/dados sensíveis].
```

### Template: Diagnóstico
```
@explore
Encontre onde [funcionalidade] está implementada.
Preciso entender: [o que procurar].
Retorne: caminhos dos arquivos e resumo da lógica.
```

## 6. Fluxo de Trabalho Recomendado

1. **Planeje** (Plan agent / Tab)
2. **Pesquise** (@explore) — entenda o código existente
3. **Implemente** (@dev-agent + skill) — código seguindo convenções
4. **Teste** (@tester) — unit + integration
5. **Revise** (@security-reviewer ou skill code-review)
6. **Compile e teste** — `dotnet build && dotnet test`
