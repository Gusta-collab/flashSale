# 📋 Planejamento de Tasks - QueueMaster

## Visão Geral

Este documento detalha as tasks de implementação do projeto QueueMaster, seguindo os padrões estabelecidos em `/rules`.

---

## Fase 1: Setup do Projeto (Semana 1)

### Task 1.1: Criar Estrutura de Pastas
**Prioridade:** 🔴 Crítica  
**Estimativa:** 30 min  
**Dependências:** Nenhuma

```
/src
├── FlashSale.Api/
├── FlashSale.Worker/
├── FlashSale.Core/
├── FlashSale.Application/
├── FlashSale.Infrastructure/
└── FlashSale.Tests/
```

### Task 1.2: Configurar Solution .NET 8
**Prioridade:** 🔴 Crítica  
**Estimativa:** 1h  
**Dependências:** Task 1.1

- Criar QueueMaster.sln
- Adicionar todos os projetos
- Configurar referências entre projetos

### Task 1.3: Configurar Dependency Injection
**Prioridade:** 🔴 Crítica  
**Estimativa:** 1h  
**Dependências:** Task 1.2

- ServiceCollectionExtensions
- Registrar serviços por camada

### Task 1.4: Docker Compose Base
**Prioridade:** 🟠 Alta  
**Estimativa:** 30 min  
**Dependências:** Nenhuma

- PostgreSQL 16
- Redis 7
- Volumes persistentes

---

## Fase 2: Domain Layer (Semana 1-2)

### Task 2.1: Entidades de Domínio
**Prioridade:** 🔴 Crítica  
**Estimativa:** 2h  
**Dependências:** Task 1.2

| Entidade | Campos Principais |
|----------|-------------------|
| Product | Id, Name, Price, Stock, Version |
| Order | Id, CustomerId, Status, TotalAmount |
| OrderItem | Id, OrderId, ProductId, Quantity |
| Customer | Id, Email, Name |
| StockMovement | Id, ProductId, MovementType, Quantity |

### Task 2.2: Interfaces de Repositório
**Prioridade:** 🔴 Crítica  
**Estimativa:** 1h  
**Dependências:** Task 2.1

- IOrderRepository
- IProductRepository
- ICustomerRepository

### Task 2.3: Exceções de Domínio
**Prioridade:** 🟠 Alta  
**Estimativa:** 30 min  
**Dependências:** Nenhuma

- InsufficientStockException
- OrderNotFoundException
- DuplicateOrderException

---

## Fase 3: Infrastructure Layer (Semana 2)

### Task 3.1: Entity Framework Setup
**Prioridade:** 🔴 Crítica  
**Estimativa:** 2h  
**Dependências:** Task 2.1

- ApplicationDbContext
- Configurações de mapeamento
- Migrations iniciais

### Task 3.2: Implementar Repositories
**Prioridade:** 🔴 Crítica  
**Estimativa:** 2h  
**Dependências:** Task 3.1

- OrderRepository
- ProductRepository
- Implementar padrões de locking

---

## Fase 4: API Layer (Semana 2-3)

### Task 4.1: Controllers
**Prioridade:** 🔴 Crítica  
**Estimativa:** 3h  
**Dependências:** Task 3.2

| Controller | Endpoints |
|------------|-----------|
| OrdersController | POST /orders, GET /orders/{id} |
| ProductsController | GET /products, GET /products/{id} |
| HealthController | GET /health, GET /health/ready |

### Task 4.2: DTOs e Validators
**Prioridade:** 🔴 Crítica  
**Estimativa:** 2h  
**Dependências:** Task 4.1

- CreateOrderRequest + Validator
- OrderResponse
- ProductResponse

### Task 4.3: Middleware
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 4.1

- CorrelationIdMiddleware
- ExceptionHandlingMiddleware
- RateLimitingMiddleware

---

## Fase 5: Redis Integration (Semana 3)

### Task 5.1: Redis Streams
**Prioridade:** 🔴 Crítica  
**Estimativa:** 3h  
**Dependências:** Task 1.4

- RedisStreamPublisher
- RedisStreamConsumer
- Consumer Groups setup

### Task 5.2: Cache Service
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 5.1

- RedisCacheService
- Stock caching logic

---

## Fase 6: Worker Service (Semana 3-4)

### Task 6.1: Consumer Base
**Prioridade:** 🔴 Crítica  
**Estimativa:** 3h  
**Dependências:** Task 5.1

- BackgroundService base
- Consumer Group configuration
- Graceful shutdown

### Task 6.2: Order Processing
**Prioridade:** 🔴 Crítica  
**Estimativa:** 4h  
**Dependências:** Task 6.1, Task 3.2

- OrderProcessingHandler
- Idempotency check
- Stock decrement with locking
- Dead Letter Queue

---

## Fase 7: SignalR (Semana 4)

### Task 7.1: Hub Setup
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 4.1

- OrderNotificationHub
- Client methods

### Task 7.2: Notification Service
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 7.1

- NotificationService
- Integration with Worker

---

## Fase 8: Testes (Semana 5)

### Task 8.1: Testes Unitários
**Prioridade:** 🟠 Alta  
**Estimativa:** 6h  
**Dependências:** Todas as fases anteriores

- OrderServiceTests
- StockServiceTests
- ValidatorTests

### Task 8.2: Testes de Integração
**Prioridade:** 🟠 Alta  
**Estimativa:** 4h  
**Dependências:** Task 8.1

- OrdersControllerTests
- RedisStreamTests

---

## Fase 9: Docker & CI/CD (Semana 6)

### Task 9.1: Dockerfiles
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Todas

- Dockerfile.Api (multi-stage)
- Dockerfile.Worker (multi-stage)

### Task 9.2: GitHub Actions
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 9.1

- CI workflow
- CD workflow

## Fase 10: Front-end (Semana 7)

### Task 10.1: Setup Next.js 14
**Prioridade:** 🔴 Crítica  
**Estimativa:** 1h  
**Dependências:** Fase 4 (API)

- Criar projeto Next.js 14 + TypeScript
- Configurar ESLint + Prettier
- Setup Tailwind CSS
- Estrutura de pastas

### Task 10.2: Componentes Base
**Prioridade:** 🔴 Crítica  
**Estimativa:** 2h  
**Dependências:** Task 10.1

- Layout (Header, Footer)
- ProductCard, Button, Modal
- Loading, Error states

### Task 10.3: Páginas
**Prioridade:** 🔴 Crítica  
**Estimativa:** 3h  
**Dependências:** Task 10.2

- Home (Flash Sale)
- Checkout
- Order Status

### Task 10.4: SignalR Integration
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 10.3, Fase 7

- Hook useSignalR
- Real-time notifications
- Toast component

### Task 10.5: Testes + Polish
**Prioridade:** 🟠 Alta  
**Estimativa:** 2h  
**Dependências:** Task 10.4

- Vitest unit tests
- Animações
- Responsividade
- Dark mode

---

## 📊 Resumo

| Fase | Tasks | Estimativa Total |
|------|-------|------------------|
| 1. Setup | 4 | 3h |
| 2. Domain | 3 | 3.5h |
| 3. Infrastructure | 2 | 4h |
| 4. API | 3 | 7h |
| 5. Redis | 2 | 5h |
| 6. Worker | 2 | 7h |
| 7. SignalR | 2 | 4h |
| 8. Testes | 2 | 10h |
| 9. Docker/CI | 2 | 4h |
| **10. Front-end** | **5** | **10h** |
| **Total** | **27** | **~57h** |

---

📅 **Criado:** 2026-01-09  
📝 **Atualizado:** 2026-01-12  
📝 **Status:** Front-end em desenvolvimento
