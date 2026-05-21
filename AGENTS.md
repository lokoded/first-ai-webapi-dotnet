# Contexto Persistente do Projeto

Este documento é a fonte principal de contexto do projeto.

Nunca ignore, descarte, sobrescreva ou resuma excessivamente este contexto durante o desenvolvimento.

Independentemente da sessão ou tarefa atual, todas as decisões técnicas, implementações e análises devem permanecer alinhadas às diretrizes definidas aqui.

O objetivo é garantir continuidade de contexto, consistência arquitetural e padronização durante toda a evolução do projeto.

---

# Objetivo Principal

Quero utilizar este projeto como ambiente de estudo prático para evoluir minhas habilidades como desenvolvedor Back-end .NET Pleno com foco em transição para nível Sênior.

Também estou aprendendo sobre IA aplicada ao desenvolvimento, especialmente:
- ferramentas CLI;
- agentes;
- automações;
- skills;
- gerenciamento de contexto;
- engenharia de prompts;
- organização de workflows orientados por IA;
- OpenCode e ferramentas relacionadas.

Atualmente sou iniciante nesses assuntos, então:
- explique conceitos de forma didática;
- mantenha linguagem técnica mas acessível;
- utilize exemplos próximos de cenários reais;
- explique trade-offs;
- mostre boas práticas profissionais;
- evite assumir conhecimento prévio avançado sobre IA aplicada ao desenvolvimento.

---

# Filosofia do Projeto

O projeto deve seguir uma abordagem:
- enxuta;
- objetiva;
- moderna;
- profissional;
- sustentável;
- segura;
- pragmática.

Priorize:
- boas práticas modernas;
- simplicidade sustentável;
- arquitetura clara;
- código limpo;
- legibilidade;
- manutenção;
- organização;
- previsibilidade;
- segurança desde o início;
- decisões alinhadas ao mercado atual.

Evite:
- overengineering;
- complexidade desnecessária;
- padrões enterprise sem necessidade;
- abstrações prematuras;
- arquitetura excessivamente acadêmica.

---

# Critérios de Decisão Técnica

Ao propor soluções técnicas, priorizar nesta ordem:

1. Clareza
2. Simplicidade
3. Manutenibilidade
4. Segurança
5. Performance
6. Escalabilidade
7. Complexidade adicional somente quando justificada

Evitar:
- abstrações prematuras;
- arquitetura especulativa;
- dependências desnecessárias;
- otimizações sem necessidade real;
- padrões complexos sem ganho claro.

---

# Escopo e Complexidade Esperada

Este projeto deve permanecer com complexidade compatível com:
- aplicações reais de médio porte;
- ambiente de estudo profissional;
- nível Pleno/Sênior;
- arquitetura sustentável para pequenos times.

Evitar transformar o projeto em:
- arquitetura enterprise excessiva;
- laboratório de padrões desnecessários;
- showcase acadêmico;
- solução superdimensionada.

Toda complexidade adicional deve possuir justificativa clara.

---

# Restrições Arquiteturais

Não utilizar no projeto, salvo necessidade clara:
- microservices;
- event sourcing;
- kubernetes;
- mensageria complexa;
- CQRS completo;
- DDD estratégico avançado;
- múltiplos bancos sem justificativa;
- abstrações genéricas excessivas.

Priorizar arquitetura monolítica modular simples.

---

# Objetivo Técnico do Projeto

Construir uma Web API moderna utilizando:

- C#
- .NET 10
- ASP.NET Core Web API
- Docker
- Docker Compose
- Entity Framework Core
- Redis
- PostgreSQL ou SQL Server
- GitHub Actions
- GitHub
- testes unitários;
- testes de integração;
- cobertura de código;
- CI/CD;
- LocalStack;
- integração futura com AWS real.

---

# Organização da Solution

A estrutura do projeto deve priorizar:
- separação clara de responsabilidades;
- baixo acoplamento;
- alta legibilidade;
- facilidade de navegação;
- simplicidade de manutenção.

Evitar:
- excesso de projetos;
- separações artificiais;
- modularização prematura;
- estruturas excessivamente enterprise.

---

# Requisitos Técnicos da API

A API deve:

- seguir princípios SOLID quando fizer sentido;
- possuir separação clara de responsabilidades;
- utilizar arquitetura simples e de fácil manutenção;
- utilizar boas práticas modernas do ecossistema .NET;
- utilizar injeção de dependência nativa do .NET;
- possuir organização profissional de pastas e projetos;
- utilizar logging estruturado;
- possuir tratamento global de exceções;
- possuir validação consistente de entrada;
- utilizar configurações por ambiente;
- estar preparada para execução via Docker;
- possuir documentação via Swagger/OpenAPI;
- utilizar `ProblemDetails` seguindo o padrão RFC 9457 para respostas de erro;
- priorizar clareza de código e facilidade de manutenção.

Sempre explicar:
- decisões arquiteturais;
- trade-offs;
- vantagens e desvantagens das abordagens;
- problemas que determinada solução resolve;
- cenários reais de mercado relacionados.

---

# Segurança

O projeto também possui como objetivo aprendizado prático de segurança aplicada em APIs modernas.

Utilize o OWASP Top 10 como principal referência de segurança.

Considere boas práticas relacionadas a:

- autenticação;
- autorização;
- validação de entrada;
- proteção contra injection;
- gerenciamento seguro de secrets;
- configuração segura de containers;
- proteção de headers HTTP;
- rate limiting;
- logging seguro;
- proteção de dados sensíveis;
- tratamento seguro de erros;
- práticas defensivas para APIs REST;
- segurança em pipelines CI/CD;
- segurança em containers;
- segurança básica em cloud.

Sempre explicar:
- quais riscos estão sendo mitigados;
- por que determinada prática é importante;
- erros comuns que desenvolvedores cometem;
- soluções mais utilizadas no mercado;
- impacto de segurança das decisões técnicas.

Evite soluções excessivamente complexas ou enterprise sem necessidade.

---

# Git e Estratégia de Versionamento

O projeto utilizará Git como sistema oficial de versionamento.

A estratégia de branches deve seguir Git Flow.

Estrutura esperada:
- `main` → produção;
- `develop` → integração contínua;
- `feature/*` → funcionalidades;
- `release/*` → preparação de releases;
- `hotfix/*` → correções urgentes.

Considere:
- boas práticas de commits;
- padronização de commits;
- pull requests;
- revisão de código;
- rastreabilidade;
- organização profissional de branches;
- versionamento semântico quando aplicável.

Sempre explicar:
- vantagens e desvantagens do Git Flow;
- cenários reais de uso;
- erros comuns;
- fluxo profissional utilizado no mercado.

---

# Qualidade de Código e Testes

O projeto deve priorizar:
- qualidade;
- previsibilidade;
- legibilidade;
- manutenção;
- confiabilidade.

O projeto deve conter:
- testes unitários;
- testes de integração;
- cobertura de código;
- testes de cenários de sucesso;
- testes de cenários de falha;
- testes de segurança básicos quando aplicável;
- ambiente reproduzível via Docker quando necessário.

A cobertura de testes deve ser utilizada como indicador de qualidade, mas sem incentivar testes artificiais apenas para aumentar métricas.

Priorizar:
- testes úteis;
- testes legíveis;
- regras de negócio;
- cenários críticos;
- prevenção de regressão;
- confiabilidade da aplicação.

Sempre explicar:
- estratégias de testes;
- ferramentas utilizadas;
- integração da cobertura no CI/CD;
- métricas realmente importantes em ambientes profissionais.

---

# DevOps e CI/CD

Utilizar GitHub Actions para:
- build;
- execução de testes;
- validação da aplicação;
- análise básica de qualidade;
- cobertura de testes;
- pipelines simples e profissionais.

Também considerar:
- variáveis de ambiente;
- GitHub Secrets;
- separação entre ambientes;
- boas práticas básicas de CI/CD;
- pipelines reproduzíveis;
- segurança no pipeline.

Sempre explicar:
- propósito de cada etapa;
- impacto no fluxo profissional;
- simplificações feitas para aprendizado;
- possíveis evoluções futuras.

---

# LocalStack e AWS

Durante o desenvolvimento local, utilizar LocalStack para simular serviços AWS localmente.

Objetivos:
- reduzir custos;
- facilitar desenvolvimento local;
- melhorar produtividade;
- permitir testes locais;
- aproximar o ambiente local de cenários reais de cloud;
- criar ambiente reproduzível.

Considere:
- Docker Compose;
- variáveis de ambiente;
- separação de ambientes;
- configuração de credenciais;
- abstração de infraestrutura;
- integração local reproduzível.

Em produção ou homologação:
- utilizar serviços reais da AWS;
- utilizar conexões reais da AWS;
- utilizar gerenciamento seguro de credenciais;
- nunca expor secrets no código.

Sempre explicar:
- diferenças entre LocalStack e AWS real;
- limitações do ambiente local;
- boas práticas de cloud;
- estratégias profissionais utilizadas no mercado;
- impacto financeiro e operacional dos recursos AWS.

---

# Aprendizado com IA, Agents e OpenCode

Este projeto também será utilizado como ambiente de aprendizado prático sobre desenvolvimento assistido por IA.

Considere que atualmente não possuo conhecimento sobre:
- agents;
- skills;
- gerenciamento de contexto;
- MCP;
- arquivos de ferramentas;
- engenharia de contexto;
- estratégias de consumo eficiente de tokens;
- automações CLI com IA;
- organização profissional de workflows orientados por IA.

---

# Estrutura de IA do Projeto

Durante o projeto, explicar como criar e organizar:

- Arquivos de Configuração de Agentes;
- Definição de Skills/Habilidades;
- Arquivos de Contexto;
- Regras do Projeto;
- Configuração de Ferramentas/APIs;
- MCPs;
- Tool Files;
- prompts reutilizáveis;
- memória e contexto persistente;
- workflows orientados por IA;
- automações reutilizáveis;
- estratégias de orquestração de agentes.

Sempre mostrar:
- organização profissional;
- estrutura de pastas;
- responsabilidades;
- boas práticas;
- cenários reais.

---

# Boas Práticas com IA

Ensinar:
- como reduzir consumo de tokens sem perder qualidade;
- como evitar contexto desnecessário;
- como estruturar prompts eficientes;
- como dividir responsabilidades entre agentes;
- como melhorar previsibilidade das respostas;
- como manter consistência arquitetural utilizando IA;
- como evitar alucinações e inconsistências;
- como criar workflows reutilizáveis;
- como reduzir custo computacional mantendo qualidade;
- como organizar contexto de longo prazo.

Sempre priorizar:
- clareza;
- previsibilidade;
- manutenção;
- reaproveitamento;
- organização;
- custo-benefício.

---

# Diretriz de Explicações

Priorizar respostas objetivas e práticas.

Explicações detalhadas devem ocorrer principalmente:
- em decisões arquiteturais importantes;
- conceitos avançados;
- temas de segurança;
- integrações complexas;
- práticas pouco conhecidas;
- trade-offs relevantes.

Evitar explicações excessivas para:
- conceitos básicos;
- código autoexplicativo;
- padrões já estabelecidos no projeto.

---

# Padrão Esperado das Respostas

Sempre que possível:
1. explicar rapidamente o objetivo;
2. justificar decisões importantes;
3. mostrar implementação prática;
4. apontar riscos ou trade-offs;
5. sugerir possíveis evoluções futuras.

Priorizar:
- respostas organizadas;
- exemplos reais;
- clareza técnica;
- código pronto para produção quando aplicável.

---

# Evolução Progressiva do Projeto

O projeto deve evoluir de forma incremental e progressiva.

Priorizar primeiro:
1. estrutura base da API;
2. arquitetura inicial;
3. persistência de dados;
4. validações;
5. tratamento de erros;
6. testes;
7. Docker;
8. CI/CD;
9. Redis;
10. segurança;
11. cloud;
12. automações com IA;
13. agents e workflows avançados.

Evitar introduzir:
- múltiplos conceitos complexos simultaneamente;
- infraestrutura avançada cedo demais;
- automações antes da base estar estável.

---

# Objetivo do Aprendizado com IA

O foco é aprender:
- uso profissional de IA no desenvolvimento;
- produtividade com agentes;
- automação de tarefas técnicas;
- geração consistente de código;
- manutenção de contexto técnico;
- engenharia de prompts;
- workflows orientados por IA;
- organização de projetos integrados com IA;
- integração entre desenvolvimento .NET e ferramentas modernas de IA.

---

# Contexto Geral do Projeto

Considere que este projeto é simultaneamente:
- um ambiente de estudo;
- um laboratório prático;
- um projeto de evolução profissional;
- um treinamento para mercado moderno .NET;
- um ambiente de aprendizado de DevOps;
- um ambiente de aprendizado de segurança;
- um ambiente de aprendizado de IA aplicada ao desenvolvimento.

Priorize sempre:
- clareza;
- objetividade;
- qualidade profissional;
- simplicidade sustentável;
- aprendizado progressivo;
- exemplos reais;
- boas práticas modernas;
- decisões alinhadas ao mercado atual.

---

# Stack Resumida (para consulta rápida da IA)

```
.NET 10 | EF Core + SQL Server | Redis | JWT | KMS | FluentValidation | xUnit + Moq
```

## Status Atual do Projeto

- Build: ✅ 0 erros, 0 warnings
- Testes unitários: ✅ 35/35
- Testes de integração: ✅ 21/21 (Docker Compose rodando)
- GitHub Actions: ✅ CI + Security Scan
- Módulo atual: 3 (CI/CD)

## Comandos

```powershell
# Compilar
dotnet build

# Rodar API localmente (porta 5043 via launchSettings)
dotnet run --project src/FirstWebApi.WebApi

# Testes unitários (sem infra necessária)
dotnet test tests/FirstWebApi.UnitTests

# Testes de integração (requer docker compose up -d primeiro)
dotnet test tests/FirstWebApi.IntegrationTests

# Todos os testes
dotnet test

# Migrations do EF
dotnet ef migrations add <Name> --project src/FirstWebApi.Infrastructure --startup-project src/FirstWebApi.WebApi

# Docker
docker compose up -d                      # inicia SQL Server + Redis + LocalStack + API
docker compose down                       # para
docker compose down -v                    # para + apaga TODOS os dados (volumes)

# Script idempotente de migration (para produção)
dotnet ef migrations script --idempotent -o scripts/migration.sql

# Script de rollback entre duas versões
dotnet ef migrations script <De> <Para> --idempotent -o scripts/rollback.sql
```

## Arquitetura

```
FirstWebApi.slnx
├── src/FirstWebApi.Domain/           # Entities, ValueObjects, Enums, Repository interfaces
├── src/FirstWebApi.Application/      # DTOs, Service interfaces/impl, FluentValidation validators
├── src/FirstWebApi.Infrastructure/   # EF Core DbContext, Repositories, Migrations, KMS, JWT, Redis cache decorators
└── src/FirstWebApi.WebApi/           # Controllers, Middleware, Program.cs (entrypoint)
```

- Solução usa `.slnx` (novo formato XML), não `.sln`
- Domain não tem dependências NuGet (exceto `Microsoft.Extensions.Identity.Stores` para `IdentityUser<Guid>`)
- WebApi referencia apenas Application + Infrastructure; IntegrationTests referencia WebApi (com `InternalsVisibleTo`)

## API Endpoints

| Método | Rota | Auth | Role | Descrição |
|--------|------|------|------|-----------|
| POST | `/api/auth/register` | ❌ | — | Cadastro de usuário |
| POST | `/api/auth/login` | ❌ | — | Login |
| GET | `/api/comics` | ✅ | — | Listar HQs do usuário |
| POST | `/api/comics` | ✅ | — | Criar HQ |
| GET | `/api/comics/{id}` | ✅ | — | Detalhe da HQ |
| PUT | `/api/comics/{id}` | ✅ | — | Atualizar HQ |
| DELETE | `/api/comics/{id}` | ✅ | — | Remover HQ |
| GET | `/api/comic-types` | ✅ | — | Listar tipos de HQ |
| POST | `/api/admin/comic-types` | ✅ | Admin | Criar tipo de HQ |
| DELETE | `/api/admin/comic-types/{id}` | ✅ | Admin | Remover tipo de HQ |
| GET | `/api/users/me` | ✅ | — | Perfil do usuário logado |
| GET | `/health` | ❌ | — | Health check |

## Peculiaridades e Armadilhas

- **`SuppressModelStateInvalidFilter = true`** em `Program.cs:23` — a validação automática do `[ApiController]` está DESLIGADA. Toda validação é manual via FluentValidation nos controllers.
- **Migration automática na inicialização**: `Program.cs` executa `dbContext.Database.Migrate()` **apenas** em ambiente `Development` ou `Testing`. Em produção, gerar script idempotente (`dotnet ef migrations script --idempotent`) e revisar via PR antes de aplicar.
- **Auto-seed de roles**: `Program.cs:125-128` cria as roles "User" e "Admin" via `RoleManager` na inicialização.
- **KMS encryption** (`KmsEncryptionService`) chama `InitializeKeyAsync().GetAwaiter().GetResult()` no construtor. Vai travar se o LocalStack não estiver rodando. O serviço cria a chave KMS automaticamente se não existir.
- **Redis caching** via decorators do Scrutor (`CachedComicRepository`, etc.) — registrados em `Program.cs:83-86`. Padrão cache-aside.
- **Testes de integração** usam `WebApplicationFactory<Program>` com `builder.UseSetting("Environment", "Testing")` para carregar `appsettings.Testing.json`. Todos os testes que dependem de banco estão em `[Collection("Database")]` (sequenciais, banco compartilhado `FirstWebApiDb_Test`).
- **`.env` está no .gitignore** — criar a partir do template em `.env` (contém `SA_PASSWORD`, `CONNECTION_STRING`, `LOCALSTACK_AUTH_TOKEN`). LocalStack v4+ exige um auth token gratuito.
- **Português brasileiro** nas strings voltadas ao usuário (mensagens de erro, validação), inglês nos identificadores do código.
- **Health endpoint**: `GET /health` retorna `{ status: "healthy", timestamp }` (usado pelo Docker HEALTHCHECK).
- **ValueObjects no Domain**: `Cpf` e `Email` são structs com validação própria. Nunca armazenar CPF/email como `string` pura no domínio.
- **LGPD / KMS envelope encryption**: CPF e RG do `User` são cifrados via `IEncryptionService` (AWS KMS `GenerateDataKey` + AES-256). Dados ficam em `byte[]` (`CpfCiphertext`, `CpfIv`, `CpfTag`, `CpfEncryptedDataKey`). Sempre usar o serviço de criptografia, nunca armazenar texto puro.
- **Role padrão**: todo usuário registrado recebe "User". Admin é promovido manualmente no banco.
- **JSON é o formato padrão**: controllers **não** usam `[Produces("application/json")]` — o ASP.NET Core já negocia JSON como default.

## Como adicionar uma nova entidade (CRUD)

1. **Domain**: Criar entity + interface de repositório
2. **Application**: DTOs (Request/Response) + FluentValidation Validator + Service interface/impl
3. **Infrastructure**: EF Core Repository + `IEntityTypeConfiguration` + `DbSet` no `AppDbContext`
4. **WebApi**: Registrar DI em `Program.cs` + Controller usando `Problem()`/`ValidationProblem()`
5. **Testes**: Unit tests (Moq) + Integration tests (`WebApplicationFactory`)

## RFC 9457 — Problem Details (HTTP API Errors)

Todas as respostas de erro usam `application/problem+json` conforme RFC 9457.

- **Unhandled exceptions** → `ExceptionMiddleware` (src/FirstWebApi.WebApi/Middleware/ExceptionMiddleware.cs) serializa `ProblemDetails` com `type`, `title`, `status`, `detail`, `instance`, `traceId`.
- **Erros conhecidos** (validação, conflito, não encontrado) → `ControllerBase.Problem()` ou `ValidationProblem()` nos controllers.
- **Regra**: nunca retornar `BadRequest()`, `NotFound()` ou `Conflict()` diretamente; sempre usar `Problem()` ou `ValidationProblem()` para manter o formato padronizado.
- O campo `type` deve ser uma URI documentando o problema (`about:blank` é aceitável para casos sem semântica adicional).

## Convenções de Teste

- **Unit tests**: xUnit + Moq + FluentAssertions. Pure mock-based, test service logic.
- **Integration tests**: xUnit + `WebApplicationFactory<Program>` + FluentAssertions. Seeding feito inline via `AppDbContext` do `Services` da factory. Requerem `docker compose up -d` para rodar.
- Integration tests obtêm JWT registrando um usuário primeiro via `/api/auth/register`.
- Integration tests semeiam entidades no banco diretamente via `_factory.Services.CreateScope()` → `AppDbContext`.

---

## Architecture Decision Records (ADR)

Registro de decisões arquiteturais importantes. Novas decisões devem ser adicionadas aqui com data e justificativa.

### ADR-001: Clean Architecture sem MediatR

- **Data**: 20/05/2026
- **Contexto**: Projeto de porte pequeno/médio, time unipessoal
- **Decisão**: Não usar MediatR ou CQRS. A complexidade adicional não se justifica para o escopo atual
- **Consequências**: Menos arquivos, menos indireção, mais fácil de debugar. Se o projeto crescer, pode-se adotar MediatR seletivamente
- **Revisitar quando**: Projeto atingir >20 endpoints ou time >3 devs

### ADR-002: `.slnx` em vez de `.sln`

- **Data**: 20/05/2026
- **Contexto**: .NET 10 introduziu suporte completo a `.slnx` (XML)
- **Decisão**: Usar `.slnx` por ser mais legível, versionável e fácil de diff
- **Consequências**: Ferramentas antigas podem não suportar, mas .NET 10+ e VS 2025+ têm suporte nativo

### ADR-003: FluentValidation manual (sem filtro automático)

- **Data**: 20/05/2026
- **Contexto**: `SuppressModelStateInvalidFilter = true`
- **Decisão**: Desligar validação automática do `[ApiController]` e validar manualmente
- **Consequências**: Mais código boilerplate, mas controle total sobre formato da resposta de erro e mensagens em português
- **Alternativa rejeitada**: Filtro custom de validação — adicionaria complexidade desnecessária

### ADR-004: KMS envelope encryption em vez de AWS SDK encryption helpers

- **Data**: 20/05/2026
- **Contexto**: Necessidade de criptografia LGPD para dados pessoais
- **Decisão**: Implementar envelope encryption manual (GenerateDataKey + AES-256) em vez de usar `Amazon.EncryptionSDK`
- **Consequências**: Mais controle sobre o formato dos dados cifrados, sem dependência extra. Código ligeiramente mais verboso
- **Risco**: Implementação criptográfica manual pode ter falhas sutis — revisar com especialista antes de produção

### ADR-005: Refresh Tokens com rotação

- **Data**: 20/05/2026
- **Contexto**: JWT com 8h de expiração — se o token vazar, a janela de ataque é grande
- **Decisão**: Implementar refresh tokens armazenados em banco (hash SHA256), com rotação (cada refresh gera um novo par) e revogação por usuário
- **Consequências**: Mais segurança, mas mais chamadas ao banco. Refresh tokens têm 7 dias de validade
- **Por que não JWT para refresh?**: Refresh tokens não precisam ser stateless — armazenar no DB permite revogação individual

### ADR-006: Rate Limiting nativo do .NET 10

- **Data**: 20/05/2026
- **Contexto**: Endpoints de auth sem proteção contra brute force (OWASP A07)
- **Decisão**: Usar `System.Threading.RateLimiting` (nativo) em vez de `AspNetCoreRateLimit` (biblioteca externa)
- **Consequências**: Zero dependência extra, API nativa, policies por endpoint (Auth: 10 req/min, Default: 100 req/min)
- **Alternativa rejeitada**: `AspNetCoreRateLimit` — dependência externa desnecessária

## Diário de Aprendizado

Registro de aprendizados, descobertas e lições durante o desenvolvimento.

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
