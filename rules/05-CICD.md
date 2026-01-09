# 🚀 05 - CI/CD Pipeline

## Integração e Deploy Contínuos

---

## 1. Visão Geral do Pipeline

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│  Build  │───▶│  Test   │───▶│Security │───▶│ Docker  │───▶│ Deploy  │
│         │    │  Unit   │    │  Scan   │    │  Build  │    │         │
└─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘
     │              │              │              │              │
   2 min          3 min          2 min          3 min          2 min
```

---

## 2. GitHub Actions Workflow

```yaml
# .github/workflows/ci-cd.yml
name: QueueMaster CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  # ════════════════════════════════════════
  # BUILD
  # ════════════════════════════════════════
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release

  # ════════════════════════════════════════
  # TESTES UNITÁRIOS
  # ════════════════════════════════════════
  test-unit:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet test --filter Category=Unit --collect:"XPlat Code Coverage"
      - uses: codecov/codecov-action@v3

  # ════════════════════════════════════════
  # TESTES DE INTEGRAÇÃO
  # ════════════════════════════════════════
  test-integration:
    needs: build
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: postgres
        ports: ['5432:5432']
      redis:
        image: redis:7
        ports: ['6379:6379']
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet test --filter Category=Integration

  # ════════════════════════════════════════
  # SECURITY SCAN
  # ════════════════════════════════════════
  security:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet list package --vulnerable
      - uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          severity: 'CRITICAL,HIGH'

  # ════════════════════════════════════════
  # DOCKER BUILD
  # ════════════════════════════════════════
  docker:
    needs: [test-unit, test-integration, security]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/build-push-action@v5
        with:
          context: ./src/FlashSale.Api
          push: true
          tags: ghcr.io/${{ github.repository }}/api:${{ github.sha }}

  # ════════════════════════════════════════
  # DEPLOY
  # ════════════════════════════════════════
  deploy:
    needs: docker
    if: github.ref == 'refs/heads/main'
    environment: production
    runs-on: ubuntu-latest
    steps:
      - run: kubectl set image deployment/api api=${{ github.sha }}
```

---

## 3. Ambientes

| Ambiente | Branch | Deploy | Aprovação |
|----------|--------|--------|-----------|
| Development | develop | Automático | Não |
| Staging | release/* | Automático | Não |
| Production | main | Manual | 2 approvers |

---

## 4. Status Checks Obrigatórios

### Para merge em develop:
- ✅ `build`
- ✅ `test-unit`

### Para merge em main:
- ✅ `build`
- ✅ `test-unit`
- ✅ `test-integration`
- ✅ `security`

---

## 5. Secrets do GitHub

```
GITHUB_TOKEN          ← Automático
CODECOV_TOKEN         ← Cobertura de código
SONAR_TOKEN           ← Análise estática
DOCKER_PASSWORD       ← Push de imagens
KUBE_CONFIG           ← Deploy Kubernetes
```

---

## 6. Rollback

```bash
# Ver deployments anteriores
kubectl rollout history deployment/api

# Rollback para versão anterior
kubectl rollout undo deployment/api

# Rollback para versão específica
kubectl rollout undo deployment/api --to-revision=3
```

---

📅 **Referência:** GitHub Actions + Kubernetes
