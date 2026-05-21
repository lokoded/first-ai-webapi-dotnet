# OWASP Top 10 2021 — Assessment FirstWebApi

> Análise de segurança do projeto contra OWASP Top 10.
> Data: 20/05/2026 | Responsável: OpenCode Security Reviewer

---

## A01 — Broken Access Control ✅/⚠️

| Item | Status |
|------|--------|
| JWT com roles (User/Admin) | ✅ |
| `[Authorize]` nos controllers | ✅ |
| Refresh tokens | ❌ **Implementar** |
| Account lockout | ❌ **Configurar** |

## A02 — Cryptographic Failures ✅

| Item | Status |
|------|--------|
| KMS envelope encryption | ✅ |
| CPF/RG/Endereço cifrados | ✅ |
| KMS sync init no construtor | ⚠️ Risco de deadlock |
| JWT HMAC-SHA256 | ✅ |

## A03 — Injection ✅

| Item | Status |
|------|--------|
| EF Core (SQL injection protegido) | ✅ |
| FluentValidation em todos inputs | ✅ |
| CPF validation algorithm | ✅ |

## A04 — Insecure Design ❌

| Item | Status |
|------|--------|
| Rate limiting | ❌ **Implementar** |
| Password reset | ⬜ Futuro |
| Validação de entrada | ✅ |

## A05 — Security Misconfiguration ⚠️

| Item | Status |
|------|--------|
| CORS AllowAll | ❌ **Restringir por ambiente** |
| Security Headers (HSTS, X-Content-Type-Options) | ❌ **Implementar** |
| appsettings.json com credenciais de produção | ❌ **Remover** |
| Exception middleware seguro | ✅ |
| HTTPS redirection | ❌ **Implementar** |

## A06 — Vulnerable Components ✅

| Item | Status |
|------|--------|
| .NET 10 latest | ✅ |
| SonarLint/Trivy no CI | ⬜ Futuro (Módulo 3) |

## A07 — Auth Failures ⚠️

| Item | Status |
|------|--------|
| Account lockout (força bruta) | ❌ **Configurar** |
| Refresh tokens | ❌ **Implementar** |
| 2FA | ⬜ Futuro |

## A09 — Security Logging ⚠️

| Item | Status |
|------|--------|
| FileLogger com TraceId | ✅ |
| Logs expõem dados sensíveis? | ⚠️ **Revisar** |
| Audit trail | ⬜ Futuro (Módulo 5) |

---

## Plano de Correção (Ordem de Implementação)

1. **CORS por ambiente** — AllowAll só em dev
2. **Security Headers** — HSTS, X-Content-Type-Options, X-Frame-Options
3. **appsettings.json** — remover credenciais sensíveis
4. **Account Lockout** — configurar Identity
5. **Rate Limiting** — middleware nativo .NET 10
6. **Refresh Tokens** — entidade + service + endpoint
7. **Logging review** — garantir que não expõe dados sensíveis
8. **HTTPS** — redirection em produção
