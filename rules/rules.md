# 📜 QueueMaster - Regras de Desenvolvimento

## Documento de Governança e Padrões do Projeto

**Versão:** 1.0.0  
**Última Atualização:** Janeiro 2026  
**Status:** ✅ Em Vigor

---

## 📑 Índice

1. [Objetivos e Progresso do Projeto](#1-objetivos-e-progresso-do-projeto)
2. [Padrões Clean Code](#2-padrões-clean-code)
3. [Desenvolvimento Seguro (DevSecOps)](#3-desenvolvimento-seguro-devsecops)
4. [Versionamento GitHub](#4-versionamento-github)
5. [CI/CD Pipeline](#5-cicd-pipeline)
6. [Testes Automatizados](#6-testes-automatizados)
7. [Docker e Containerização](#7-docker-e-containerização)
8. [Documentação de Código](#8-documentação-de-código)
9. [Regras de Engenharia](#9-regras-de-engenharia)
10. [Checklist de Code Review](#10-checklist-de-code-review)

---

## 1. Objetivos e Progresso do Projeto

### 1.1 Objetivo Geral

Desenvolver um sistema de vendas de alta demanda (Flash Sale) capaz de processar 
milhares de requisições por segundo com garantia de consistência de dados.

### 1.2 Marcos do Projeto (Milestones)

| Marco | Descrição | Status | Prazo |
|-------|-----------|--------|-------|
| M1 | Estrutura base do projeto (Clean Architecture) | 🔲 Pendente | Semana 1 |
| M2 | Implementação da API (Producer) | 🔲 Pendente | Semana 2 |
| M3 | Implementação do Worker (Consumer) | 🔲 Pendente | Semana 3 |
| M4 | Integração Redis Streams | 🔲 Pendente | Semana 4 |
| M5 | SignalR (Notificações Real-time) | 🔲 Pendente | Semana 5 |
| M6 | Testes Unitários e Integração | 🔲 Pendente | Semana 6 |
| M7 | Testes de Carga (k6) | 🔲 Pendente | Semana 7 |
| M8 | Docker e CI/CD | 🔲 Pendente | Semana 8 |
| M9 | Documentação Final | 🔲 Pendente | Semana 9 |

### 1.3 Métricas de Sucesso

```
✅ Suportar 5.000+ requisições/segundo
✅ Latência P95 < 500ms
✅ Taxa de erro < 0.1%
✅ Zero race conditions (estoque nunca negativo)
✅ Cobertura de testes > 80%
✅ Zero vulnerabilidades críticas (OWASP)
```

---

## 2. Padrões Clean Code

### 2.1 Nomenclatura

```csharp
// ✅ CORRETO: Nomes descritivos e em inglês

// Classes: PascalCase, substantivos
public class OrderProcessingService { }
public class CustomerRepository { }

// Interfaces: PascalCase com prefixo "I"
public interface IOrderRepository { }
public interface IMessagePublisher { }

// Métodos: PascalCase, verbos
public async Task<Order> ProcessOrderAsync(Guid orderId) { }
public bool ValidateStock(int quantity) { }

// Variáveis/Parâmetros: camelCase
private readonly ILogger _logger;
public void CreateOrder(OrderRequest request) { }

// Constantes: PascalCase ou SCREAMING_SNAKE_CASE
public const int MaxRetryAttempts = 3;
public const string REDIS_STREAM_NAME = "orders:pending";

// ❌ INCORRETO
public class order_service { }          // snake_case
public void process() { }               // nome genérico
private readonly ILogger l;             // abreviação
```

### 2.2 Estrutura de Métodos

```csharp
// ✅ CORRETO: Método pequeno, faz uma coisa só

/// <summary>
/// Valida se há estoque suficiente para o pedido.
/// </summary>
/// <param name="productId">ID do produto a verificar</param>
/// <param name="requestedQuantity">Quantidade solicitada</param>
/// <returns>True se há estoque suficiente</returns>
public async Task<bool> HasSufficientStockAsync(
    Guid productId, 
    int requestedQuantity,
    CancellationToken cancellationToken = default)
{
    // 1. Buscar estoque atual no cache
    var cachedStock = await _cache.GetAsync<int?>($"stock:{productId}");
    
    // 2. Se não estiver em cache, buscar no banco
    if (cachedStock == null)
    {
        var product = await _repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);
        
        cachedStock = product.Stock;
        await _cache.SetAsync($"stock:{productId}", cachedStock, TimeSpan.FromSeconds(30));
    }
    
    // 3. Retornar comparação
    return cachedStock >= requestedQuantity;
}
```

### 2.3 Princípios SOLID

```csharp
// S - Single Responsibility
public class OrderValidator { }           // Só valida
public class OrderRepository { }          // Só persiste
public class OrderNotificationService { } // Só notifica

// O - Open/Closed
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessAsync(Payment payment);
}
public class CreditCardProcessor : IPaymentProcessor { }
public class PixProcessor : IPaymentProcessor { }

// L - Liskov Substitution
public abstract class Notification { public abstract Task SendAsync(string message); }
public class EmailNotification : Notification { }
public class SmsNotification : Notification { }

// I - Interface Segregation
public interface IOrderReader { Task<Order> GetByIdAsync(Guid id); }
public interface IOrderWriter { Task CreateAsync(Order order); }

// D - Dependency Inversion
public class OrderService
{
    private readonly IOrderRepository _repository;  // Interface, não implementação
    public OrderService(IOrderRepository repository) => _repository = repository;
}
```

### 2.4 Tratamento de Erros

```csharp
// ✅ CORRETO: Exceções específicas e bem logadas
public async Task<Order> GetOrderAsync(Guid orderId)
{
    try
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado", orderId);
            throw new OrderNotFoundException(orderId);
        }
        return order;
    }
    catch (NpgsqlException ex)
    {
        _logger.LogError(ex, "Erro de banco ao buscar pedido {OrderId}", orderId);
        throw new DatabaseException("Erro ao acessar banco de dados", ex);
    }
}

// ❌ INCORRETO: Swallow exception
public async Task<Order> GetOrderBad(Guid orderId)
{
    try { return await _repository.GetByIdAsync(orderId); }
    catch (Exception) { return null; }  // Esconde o erro!
}
```

---

## 3. Desenvolvimento Seguro (DevSecOps)

### 3.1 Regras de Segurança Obrigatórias

| Regra | Descrição | Prioridade |
|-------|-----------|------------|
| SEC-01 | Nunca commitar secrets (senhas, API keys) | 🔴 Crítica |
| SEC-02 | Usar variáveis de ambiente para configurações | 🔴 Crítica |
| SEC-03 | Validar TODA entrada do usuário | 🔴 Crítica |
| SEC-04 | Usar prepared statements (sem SQL injection) | 🔴 Crítica |
| SEC-05 | Implementar rate limiting | 🟠 Alta |
| SEC-06 | Logar todas as operações sensíveis | 🟠 Alta |
| SEC-07 | Usar HTTPS em todas as comunicações | 🟠 Alta |
| SEC-08 | Implementar autenticação JWT | 🟠 Alta |
| SEC-09 | Manter dependências atualizadas | 🟡 Média |
| SEC-10 | Executar scan de vulnerabilidades (SAST) | 🟡 Média |

### 3.2 Gerenciamento de Secrets

```yaml
# ✅ CORRETO: Usar variáveis de ambiente
# docker-compose.yml
services:
  api:
    environment:
      - ConnectionStrings__PostgreSQL=${POSTGRES_CONNECTION_STRING}
      - Redis__Password=${REDIS_PASSWORD}
      - Jwt__Secret=${JWT_SECRET}
```

```bash
# ✅ .gitignore para secrets
*.env
*.env.local
appsettings.Development.json
secrets/
```

### 3.3 Validação de Entrada

```csharp
// FluentValidation
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(10);
        });
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}
```

### 3.4 OWASP Top 10 Checklist

- [ ] A01 – Broken Access Control
- [ ] A02 – Cryptographic Failures
- [ ] A03 – Injection
- [ ] A04 – Insecure Design
- [ ] A05 – Security Misconfiguration
- [ ] A06 – Vulnerable Components
- [ ] A07 – Auth Failures
- [ ] A08 – Software Integrity
- [ ] A09 – Security Logging
- [ ] A10 – SSRF

---

## 4. Versionamento GitHub

### 4.1 Estrutura de Branches

```
main          ← Produção (protegida)
├── develop   ← Integração (protegida)
│   ├── feature/QM-001-order-api
│   ├── bugfix/QM-042-stock-fix
│   └── hotfix/QM-099-security
└── release/v1.0.0
```

### 4.2 Conventional Commits

```bash
# Formato: <type>(<scope>): <description>

feat(api): add order creation endpoint
fix(worker): resolve race condition in stock decrement
docs(readme): update installation instructions
test(order): add unit tests for OrderService
chore(deps): update Entity Framework to 8.0.1
security(auth): fix JWT token validation
perf(redis): optimize stream reading
```

### 4.3 Pull Request Template

```markdown
## Descrição
[O que foi alterado e por quê]

## Tipo de Mudança
- [ ] 🆕 Feature | [ ] 🐛 Bugfix | [ ] 🔒 Security | [ ] ♻️ Refactor

## Checklist
- [ ] Código segue padrões do projeto
- [ ] Testes adicionados/atualizados
- [ ] Documentação atualizada
- [ ] Sem secrets commitados
```

### 4.4 Proteção de Branches

```yaml
main:
  - Require 2 approvals
  - Require status checks: build, test, security-scan
  - No bypass allowed

develop:
  - Require 1 approval
  - Require status checks: build, test
```

---

## 5. CI/CD Pipeline

### 5.1 Workflow GitHub Actions

```yaml
name: QueueMaster CI/CD

on:
  push: [main, develop]
  pull_request: [main, develop]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet restore
      - run: dotnet build --configuration Release

  test-unit:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test --filter Category=Unit --collect:"XPlat Code Coverage"
      - uses: codecov/codecov-action@v3

  test-integration:
    needs: build
    runs-on: ubuntu-latest
    services:
      postgres: { image: postgres:16 }
      redis: { image: redis:7 }
    steps:
      - run: dotnet test --filter Category=Integration

  security:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - run: dotnet list package --vulnerable
      - uses: aquasecurity/trivy-action@master

  docker:
    needs: [test-unit, test-integration, security]
    if: github.ref == 'refs/heads/main'
    steps:
      - run: docker build -t api:${{ github.sha }} ./src/FlashSale.Api
      - run: docker push ghcr.io/${{ github.repository }}/api:${{ github.sha }}

  deploy:
    needs: docker
    if: github.ref == 'refs/heads/main'
    environment: production
    steps:
      - run: kubectl set image deployment/flashsale-api api=${{ github.sha }}
```

---

## 6. Testes Automatizados

### 6.1 Estrutura de Testes

```
/src/FlashSale.Tests
├── /Unit           ← Testes unitários
├── /Integration    ← Testes de integração
├── /Load           ← Testes de carga (k6)
└── /E2E            ← Testes end-to-end
```

### 6.2 Padrão AAA

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task ProcessOrder_WithSufficientStock_ShouldConfirmOrder()
{
    // ═══════════════════════════════════════
    // ARRANGE: Preparar cenário
    // ═══════════════════════════════════════
    var product = new Product { Id = Guid.NewGuid(), Stock = 10 };
    _mockRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);
    
    // ═══════════════════════════════════════
    // ACT: Executar ação
    // ═══════════════════════════════════════
    var result = await _orderService.ProcessOrderAsync(request);
    
    // ═══════════════════════════════════════
    // ASSERT: Verificar resultado
    // ═══════════════════════════════════════
    Assert.Equal(OrderStatus.Confirmed, result.Status);
}
```

### 6.3 Cobertura Mínima

| Camada | Cobertura |
|--------|-----------|
| Core (Domínio) | 90% |
| Application | 85% |
| Infrastructure | 70% |
| API | 75% |

### 6.4 k6 Load Test

```javascript
export const options = {
  scenarios: {
    flash_sale: {
      executor: 'ramping-vus',
      stages: [
        { duration: '10s', target: 100 },
        { duration: '30s', target: 5000 },
        { duration: '10s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],
    errors: ['rate<0.01'],
  },
};
```

---

## 7. Docker e Containerização

### 7.1 Dockerfile Multi-stage

```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
RUN adduser -S appuser
WORKDIR /app
COPY --from=build /app .
USER appuser
EXPOSE 8080
HEALTHCHECK CMD wget -q --spider http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FlashSale.Api.dll"]
```

### 7.2 Docker Compose

```yaml
version: '3.8'
services:
  api:
    build: ./src/FlashSale.Api
    ports: ["5000:8080"]
    depends_on: [postgres, redis]
  
  worker:
    build: ./src/FlashSale.Worker
    deploy: { replicas: 3 }
    depends_on: [postgres, redis]
  
  postgres:
    image: postgres:16-alpine
    volumes: [postgres_data:/var/lib/postgresql/data]
  
  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes

volumes:
  postgres_data:
```

### 7.3 Comandos Essenciais

```bash
docker-compose up -d              # Subir ambiente
docker-compose logs -f api worker # Ver logs
docker-compose up -d --scale worker=10  # Escalar workers
docker-compose down               # Parar tudo
```

---

## 8. Documentação de Código

### 8.1 XML Docs Obrigatório

```csharp
/// <summary>
/// Processa um pedido de forma assíncrona, garantindo idempotência.
/// </summary>
/// <param name="request">Dados do pedido a ser processado.</param>
/// <param name="cancellationToken">Token para cancelamento.</param>
/// <returns>Resultado do processamento.</returns>
/// <exception cref="InsufficientStockException">Estoque insuficiente.</exception>
public async Task<OrderResult> ProcessOrderAsync(
    CreateOrderRequest request,
    CancellationToken cancellationToken = default)
```

### 8.2 Comentários Inline

```csharp
public async Task<bool> DecrementStockAsync(Guid productId, int quantity)
{
    // ═══════════════════════════════════════
    // PASSO 1: Verificar cache primeiro
    // Evita hit no banco para consultas frequentes
    // ═══════════════════════════════════════
    var cachedStock = await _cache.GetAsync<int?>($"stock:{productId}");
    
    if (cachedStock.HasValue && cachedStock.Value < quantity)
        return false; // Early return
    
    // ═══════════════════════════════════════
    // PASSO 2: Decrementar com lock otimista
    // ═══════════════════════════════════════
    // ... continua
}
```

---

## 9. Regras de Engenharia

### 9.1 Regras de Código

| ID | Regra | Severidade |
|----|-------|------------|
| ENG-01 | Documentação XML em membros públicos | 🔴 Error |
| ENG-02 | Métodos max 30 linhas | 🟠 Warning |
| ENG-03 | Classes max 300 linhas | 🟠 Warning |
| ENG-04 | Complexidade ciclomática max 10 | 🟠 Warning |
| ENG-05 | Usar async/await para I/O | 🔴 Error |
| ENG-06 | Não bloquear threads (.Result) | 🔴 Error |
| ENG-07 | CancellationToken em async | 🟠 Warning |
| ENG-08 | Injeção via construtor | 🔴 Error |

### 9.2 Dependências de Arquitetura

```
FlashSale.Api          → Application, Core, Infrastructure
FlashSale.Worker       → Application, Core, Infrastructure
FlashSale.Application  → Core (❌ NÃO Infrastructure)
FlashSale.Core         → Ninguém (zero deps)
FlashSale.Infrastructure → Core
```

### 9.3 Definition of Done

```
☐ Código implementado (Clean Code)
☐ Testes unitários passando
☐ Testes integração passando
☐ Documentação XML
☐ Code review aprovado
☐ Sem vulnerabilidades críticas
☐ Pipeline CI/CD passando
☐ Migrations criadas (se aplicável)
```

---

## 10. Checklist de Code Review

```markdown
### Funcionalidade
- [ ] Código faz o que deveria?
- [ ] Edge cases considerados?

### Clean Code
- [ ] Nomes descritivos?
- [ ] Funções pequenas?
- [ ] Sem duplicação?

### Segurança
- [ ] Inputs validados?
- [ ] Sem secrets hardcoded?

### Testes
- [ ] Testes existem?
- [ ] Cobertura adequada?

### Performance
- [ ] Queries otimizadas?
- [ ] Cache apropriado?

### Documentação
- [ ] XML docs presentes?
- [ ] README atualizado?
```

---

## ✅ Resumo das Regras

| Área | Regra Chave |
|------|-------------|
| **Clean Code** | Nomes descritivos, funções pequenas, SOLID |
| **Segurança** | Nunca secrets, sempre validar, sempre HTTPS |
| **Versionamento** | Conventional Commits, feature branches |
| **Testes** | Cobertura 80%, padrão AAA, k6 para carga |
| **Documentação** | XML em públicos, comentários explicam "porquê" |

---

📅 **Próxima revisão:** Após M3 (Implementação do Worker)
