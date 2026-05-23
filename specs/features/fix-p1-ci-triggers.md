# Fix: CI Triggers — Adicionar develop, feature/*, fix/* (P1)

## Objetivo

Corrigir triggers do CI e security scan para incluir `develop`, `feature/*` e `fix/*`.

## Regras de Negócio

1. `ci.yml`: push triggers para `develop`, `main`, `feature/*`, `fix/*`
2. `security.yml`: push triggers para `develop`, `main`

## Arquivos Afetados

- `.github/workflows/ci.yml`
- `.github/workflows/security.yml`

## Critérios de Aceitação

- [ ] CI dispara em push para `develop`, `feature/*`, `fix/*`, `main`
- [ ] Security scan dispara em push para `develop` e `main`
- [ ] PRs de `feature/*` para `develop` disparam CI
