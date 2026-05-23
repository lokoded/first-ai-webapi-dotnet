# 08 — Anti-Patterns (O Que NÃO Fazer)

## 1. Generic Repository + Unit of Work sobre EF Core

EF Core já implementa Repository (`DbSet`) e Unit of Work (`DbContext.SaveChangesAsync`). Adicionar outra camada é abstração sem ganho.

**Quando aceitável**: Se precisar de um decorator de cache (Redis) ou se o repositório tiver regras de negócio específicas além do CRUD básico.

## 2. CQRS/MediatR para CRUD simples

CQRS adiciona: +1 arquivo por handler, +1 por query/command, +1 por validator. Para um CRUD de 5 endpoints, são ~20 arquivos onde 5 bastariam.

**Quando aceitável**: >20 endpoints ou time >3 devs (ver ADR-001).

## 3. Event Sourcing sem requisito de auditoria

Persistir eventos em vez de estado atual é 10x mais complexo. Só faz sentido se você PRECISA de audit trail completo ou reconstrução de estado histórico.

## 4. Microservices sem escala necessária

Microservices = overhead de rede, consistência eventual, deploy complexo, debugging difícil. Um monolito modular bem estruturado resolve 90% dos casos.

## 5. God classes / Services >300 linhas

Método com uma responsabilidade só. Classe com uma responsabilidade só. Se precisa de "e" para descrever, divida.

## 6. Magic strings ou números

Use constantes com nome descritivo ou enums. String literal espalhada = pesadelo de manutenção.

## 7. Lazy Loading em Web API

Lazy loading = N+1 queries. Cada acesso a uma propriedade de navegação dispara uma nova query SQL. Sempre `Include()` explícito.

## 8. Abstrações Prematuras

"Não antecipe necessidades futuras. Implemente o que foi pedido, não o que você acha que vai precisar." (YAGNI)

## 9. Tratamento de Erro com Try/Catch nos Controllers

O middleware de exceção global captura tudo. Try/catch no controller é boilerplate inútil e esquece de padronizar a resposta.

## 10. Propagar Entidades do Domain para a View

Nunca retornar entidades do Domain nos controllers. Usar DTOs. Entidades expõem mais dados que o necessário e acoplam a API ao modelo de dados interno.
