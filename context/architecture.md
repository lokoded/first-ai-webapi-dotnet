# Arquitetura do Projeto

> Fonte: AGENTS.md (extraído em 21/05/2026)

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

## Estrutura de Pastas

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

---

## RFC 9457 — Problem Details (HTTP API Errors)

Todas as respostas de erro usam `application/problem+json` conforme RFC 9457.

- **Unhandled exceptions** → `AppExceptionHandler` (implements `IExceptionHandler`) captura e serializa `ProblemDetails` com `type`, `title`, `status`, `detail`, `instance`, `traceId`.
- **Erros conhecidos** (validação, conflito, não encontrado) → `ControllerBase.Problem()` ou `ValidationProblem()` nos controllers.
- **Regra**: nunca retornar `BadRequest()`, `NotFound()` ou `Conflict()` diretamente; sempre usar `Problem()` ou `ValidationProblem()` para manter o formato padronizado.
- O campo `type` deve ser uma URI documentando o problema (`about:blank` é aceitável para casos sem semântica adicional).

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

### ADR-007: DadoProtegido — blob único opaco para dados cifrados

- **Data**: 2026-05-22
- **Status**: Accepted
- **Contexto**: Dados cifrados eram armazenados em 4 colunas `byte[]` por campo
  (Ciphertext, IV, Tag, EncryptedDataKey), expondo primitivas de crypto
  (AES-GCM + envelope KMS) no Domain — violação de Clean Architecture.
- **Decisão**:
  1. `Domain/ValueObjects/DadoProtegido(byte[] Valor)` — blob único opaco,
     sem qualquer detalhe de algoritmo criptográfico
  2. `EncryptedData(Ciphertext, Iv, Tag, EncryptedDataKey)` movido para
     Infrastructure como tipo interno de serialização
  3. `IEncryptionService` passa a retornar/trafegar `DadoProtegido`
  4. Entities (User, Address): 1 propriedade `byte[]?` por campo cifrado
     (ex: `CpfDados`, `RgDados`, `Dados`) em vez de 4 propriedades
  5. EF Core ValueConverter empacota/desempacota `EncryptedData ↔ byte[]`
     transparentemente
- **Consequências**:
  - Domain não conhece algoritmo de criptografia
  - Trocar algoritmo (ex: AES-GCM → ChaCha20) = só alterar Infrastructure
  - Migration única para consolidar colunas: 12→3 (User), 4→1 (Address)
  - Perda da visibilidade dos componentes da cifra no banco (aceitável)
- **Alternativa rejeitada**: Manter 4 colunas com renomeação genérica
  ("ParametroA", "ParametroB") — abstração hipócrita, fere Clareza
- **Alternativa rejeitada**: Manter status quo (12 colunas) — fere Clean Architecture
- **Mercado**: Prática padrão — EF Core ValueConverters (Microsoft docs),
  Amazon DynamoDB Encryption Client, projetos Clean Architecture reais
