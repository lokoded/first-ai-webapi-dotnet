# 02 — Segurança (OWASP Top 10 + LGPD)

## A01 — Broken Access Control

- `[Authorize]` global no controller; opt-out com `[AllowAnonymous]`
- UserId sempre do token autenticado, nunca de request body/query
- Validar ownership do recurso: o usuário autenticado é dono deste recurso?
- Admin endpoints em controller separado com `[Authorize(Roles = "Admin")]`

## A02 — Cryptographic Failures

- HTTPS obrigatório: `UseHsts()` + `UseHttpsRedirection()`
- Senhas: Identity hasher padrão (PBKDF2)
- Dados sensíveis (CPF, RG, endereço): cifrados com `IEncryptionService` (KMS envelope encryption)
- **Nunca** passar dados sensíveis em query strings
- `DadoProtegido` é blob opaco — Domain/Application nunca acessam `Valor` diretamente

## A03 — Injection

- Sempre EF Core ou queries parametrizadas. Zero concatenação de SQL.
- Validar com FluentValidation + allow-lists para valores enumeráveis
- Nunca passar input do usuário para `Path.Combine`, `Process.Start` ou comandos shell

## A04 — Insecure Design

- Rate limiting em endpoints de auth (10 req/min) e público (100 req/min)
- Account lockout: 5 tentativas falhas, 15 minutos de bloqueio
- Defesa em profundidade: auth + autorização + validação em camadas separadas

## A05 — Security Misconfiguration

- Security headers: HSTS, X-Content-Type-Options, X-Frame-Options, CSP
- **Nunca** `AllowAnyOrigin` + `AllowCredentials` juntos (CORS)
- Swagger oculto ou protegido em produção
- CORS configurado por ambiente

## A06 — Vulnerable Components

- `dotnet list package --vulnerable` no CI
- Dependabot habilitado no GitHub
- Remover pacotes NuGet não utilizados

## A07 — Authentication Failures

- Access token JWT: 8h de expiração, HMAC-SHA256
- Refresh token: HttpOnly + Secure + SameSite=Strict cookie (nunca JSON body)
- `ClockSkew = TimeSpan.Zero` (padrão do .NET aceita 5 min de tolerância)
- **Nunca** armazenar tokens em localStorage (XSS-vulnerável)

## A08 — Software and Data Integrity

- Nunca `Newtonsoft TypeNameHandling.All` com dados externos (RCE)
- Nunca `BinaryFormatter` (inseguro em todas as versões)
- Usar `System.Text.Json` com resolvedores de tipo explícitos

## A09 — Security Logging and Monitoring

- Logar toda falha de autenticação e acesso negado (IP, user-agent, timestamp)
- **Nunca** logar: senhas, tokens, CPF, RG, cartão ou qualquer dado pessoal
- Sink externo recomendado: Seq, App Insights ou Elastic (com alertas)

## A10 — SSRF

- Validar URLs fornecidas pelo usuário antes de passar para `HttpClient`
- Allow-list de domínios permitidos para chamadas HTTP externas
- Bloquear IPs internos, localhost e endpoints de metadata cloud (169.254.x.x)
- Sempre configurar timeout e `MaxResponseContentBufferSize` no `HttpClient`

## LGPD — Dados Pessoais

- CPF, RG e endereço sempre cifrados em repouso via `IEncryptionService`
- Logs nunca contêm dados pessoais em texto puro
- Migration de dados cifrados exige cuidado com formato do blob

## JWT

- Chave de assinatura: mínimo 32 caracteres, em env var ou secrets manager
- JWT é Base64 (não cifrado) — nunca colocar dados sensíveis no payload
- `ValidateLifetime = true`, `ClockSkew = TimeSpan.Zero`
- `UseAuthentication()` antes de `UseAuthorization()` no pipeline
- Roles para hierarquia simples, Policies para regras complexas
