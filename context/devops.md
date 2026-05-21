# DevOps e Infraestrutura

> Fonte: AGENTS.md (extraído em 21/05/2026)

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
