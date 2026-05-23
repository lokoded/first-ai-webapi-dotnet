# 05 — Logging e Serialização

## Logging Estruturado

- Injetar `ILogger<T>` via DI. Nunca usar `Log.Logger` diretamente em services.
- **Nunca** usar interpolação em mensagens de log:
  ```csharp
  // Correto
  _logger.LogInformation("Pedido {PedidoId} criado", pedido.Id);
  
  // Errado — destrói logging estruturado
  _logger.LogInformation($"Pedido {pedido.Id} criado");
  ```

## Níveis de Log

| Nível | Uso |
|-------|-----|
| Debug | Desenvolvimento apenas |
| Information | Eventos de negócio (pedido criado, usuário registrado) |
| Warning | Problemas recuperáveis (Redis offline, fallback) |
| Error | Exceções — sempre passar a exceção como primeiro argumento |
| Critical | Sistema indisponível |

## O Que NUNCA Logar

- Senhas em qualquer formato
- Tokens JWT ou refresh tokens
- CPF, RG, endereço ou qualquer dado pessoal (mesmo cifrado)
- Números de cartão de crédito
- Secrets, API keys, connection strings
- Stack traces detalhados em produção (Exception middleware cuida disso)

## Enriquecer Logs

- RequestId (`HttpContext.TraceIdentifier`) em toda request
- UserId quando autenticado
- Adicionar em middleware, não em cada controller

## Supressão de Namespaces

- EF Core → Warning em produção (evitar ruído de queries)
- Suprimir logs de Microsoft/System em produção

## Sink

- Desenvolvimento: console + arquivo (`logs/`)
- Produção: Seq, Application Insights ou Elastic (com alertas configurados)

## Serialização

- `System.Text.Json` padrão do ASP.NET
- Propriedades camelCase (padrão)
- `JsonIgnore` em dados sensíveis que não devem sair na resposta
- Evitar loops de referência com `ReferenceHandler.IgnoreCycles`
