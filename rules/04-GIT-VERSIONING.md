# 🔀 04 - Versionamento e GitHub

## Guia de Controle de Versão

---

## 1. Estrutura de Branches

```
main              ← Produção (protegida, requer 2 approvals)
├── develop       ← Integração (protegida, requer 1 approval)
│   ├── feature/QM-001-order-api
│   ├── feature/QM-015-redis-streams
│   ├── bugfix/QM-042-stock-fix
│   └── hotfix/QM-099-security
└── release/v1.0.0
```

---

## 2. Naming Convention

### 2.1 Branches

| Tipo | Padrão | Exemplo |
|------|--------|---------|
| Feature | `feature/[TICKET]-descricao` | `feature/QM-001-order-api` |
| Bugfix | `bugfix/[TICKET]-descricao` | `bugfix/QM-042-stock-fix` |
| Hotfix | `hotfix/[TICKET]-descricao` | `hotfix/QM-099-security` |
| Release | `release/v[MAJOR].[MINOR].[PATCH]` | `release/v1.2.0` |

### 2.2 Commits (Conventional Commits)

```bash
# Formato
<type>(<scope>): <description>

# Types
feat:     Nova funcionalidade
fix:      Correção de bug
docs:     Documentação
style:    Formatação
refactor: Refatoração
test:     Testes
chore:    Manutenção
perf:     Performance
security: Segurança

# Exemplos
feat(api): add order creation endpoint
fix(worker): resolve race condition in stock decrement
docs(readme): update installation instructions
test(order): add unit tests for OrderService
security(auth): fix JWT token validation
```

---

## 3. Fluxo de Trabalho

```
1. Criar branch a partir de develop
   git checkout develop
   git pull origin develop
   git checkout -b feature/QM-001-order-api

2. Desenvolver com commits frequentes
   git add .
   git commit -m "feat(api): add order controller"

3. Push e criar PR
   git push origin feature/QM-001-order-api

4. Code Review (mínimo 1 approval)

5. Merge via Squash and Merge

6. Deletar branch após merge
```

---

## 4. Pull Request Template

```markdown
## Descrição
[O que foi alterado e por quê]

## Tipo de Mudança
- [ ] 🆕 Feature
- [ ] 🐛 Bugfix
- [ ] 🔒 Security
- [ ] ♻️ Refactor
- [ ] 📝 Docs

## Ticket
Closes #[NÚMERO]

## Checklist
- [ ] Código segue padrões do projeto
- [ ] Testes adicionados/atualizados
- [ ] Documentação atualizada
- [ ] Sem secrets commitados
- [ ] Self-review realizado

## Como Testar
1. Passo 1
2. Passo 2
3. Resultado esperado
```

---

## 5. Proteção de Branches

### main (Produção)
- ✅ Require pull request (2 approvals)
- ✅ Require status checks: build, test, security-scan
- ✅ Require conversation resolution
- ✅ No bypass allowed
- ✅ No force push

### develop (Integração)
- ✅ Require pull request (1 approval)
- ✅ Require status checks: build, test-unit

---

## 6. Semantic Versioning

```
MAJOR.MINOR.PATCH

MAJOR: Breaking changes
MINOR: New features (backward compatible)
PATCH: Bug fixes

Exemplos:
1.0.0 → 1.0.1  (bug fix)
1.0.1 → 1.1.0  (new feature)
1.1.0 → 2.0.0  (breaking change)
```

---

## 7. Git Hooks (Pre-commit)

```bash
# .husky/pre-commit
#!/bin/sh

# Verificar secrets
git secrets --scan

# Lint
dotnet format --verify-no-changes

# Rodar testes unitários
dotnet test --filter Category=Unit
```

---

## 8. Comandos Úteis

```bash
# Atualizar branch com develop
git checkout feature/my-feature
git rebase develop

# Squash commits locais
git rebase -i HEAD~3

# Desfazer último commit (mantendo alterações)
git reset --soft HEAD~1

# Ver histórico limpo
git log --oneline -10

# Criar tag
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

---

📅 **Referência:** Git Flow + Conventional Commits
