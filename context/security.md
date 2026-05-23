# Segurança do Projeto

> Fonte: AGENTS.md (extraído em 21/05/2026)

---

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

## Detalhes de Implementação

- **JWT**: HMAC-SHA256, 8h expiry
- **Refresh tokens**: armazenados em banco (hash SHA256), rotação a cada uso, 7 dias de validade, revogação por usuário
- **Rate limiting**: nativo .NET 10 — Auth: 10 req/min, Default: 100 req/min
- **Account lockout**: 5 tentativas, 15 min de bloqueio
- **Security headers**: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy
- **CORS**: restrito por ambiente (via configuração)
- **Criptografia LGPD**: CPF, RG e endereço cifrados via `IEncryptionService`
  (AWS KMS `GenerateDataKey` + AES-256-GCM envelope).
  Armazenados como `DadoProtegido(byte[])` — blob opaco.
  O formato interno de serialização é responsabilidade exclusiva da Infrastructure,
  não exposto ao Domain. A Application trafega `DadoProtegido`, o Domain não conhece
  o conceito de algoritmo criptográfico.
- **Role padrão**: todo usuário registrado recebe "User". Admin promovido manualmente no banco
- **Logs**: nunca contêm secrets, CPF, RG ou dados sensíveis
- **Exception middleware**: nunca expõe stack trace
- **KMS `InitializeKeyAsync().GetAwaiter().GetResult()`**: trava se LocalStack estiver offline
