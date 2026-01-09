# Changelog

Todas as mudanças notáveis do projeto QueueMaster serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).
Este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

### Planned
- 📋 Integration tests
- 📋 Load tests com k6

---

## [0.8.0] - 2026-01-09

### Added - Docker & CI/CD (Fase 9)
- ✅ `Dockerfile.api` - Multi-stage com Alpine e non-root user
- ✅ `Dockerfile.worker` - Multi-stage otimizado
- ✅ GitHub Actions workflow com build, test, security, Docker
- ✅ Trivy security scanning
- ✅ Codecov coverage upload

---

## [0.7.0] - 2026-01-09

### Added - Automated Tests (Fase 8)
- ✅ 13 testes unitários passando
- ✅ `ProductTests` - testes de estoque
- ✅ `OrderTests` - testes de pedido
- ✅ `DomainExceptionTests` - testes de exceções
- ✅ FluentAssertions + Moq configurados

---

## [0.6.0] - 2026-01-09

### Added - SignalR (Fase 7)
- ✅ `OrderNotificationHub` - Hub WebSocket para pedidos
- ✅ `SignalRNotificationService` - serviço de notificações
- ✅ Redis Backplane para escalabilidade horizontal
- ✅ Eventos: OrderConfirmed, OrderFailed, OrderStatusChanged
- ✅ README.md atualizado com documentação completa

---

## [0.5.0] - 2026-01-09

### Added - Worker Service (Fase 6)
- ✅ `OrderProcessingHandler` - processamento com locking de estoque
- ✅ `OrderConsumerService` - BackgroundService com Consumer Groups
- ✅ Graceful shutdown e retry logic
- ✅ Integração com repositories para validação de estoque

---

## [0.4.0] - 2026-01-09

### Added - Redis Integration (Fase 5)
- ✅ `RedisStreamPublisher` - publicação com XADD
- ✅ `RedisStreamConsumer` - consumo com XREADGROUP
- ✅ `RedisCacheService` - cache distribuído
- ✅ Interfaces: IStreamPublisher, IStreamConsumer, ICacheService
- ✅ Integração com OrdersController

### Added - API Layer (Fase 4)
- ✅ `OrdersController` com POST /orders e GET /orders/{id}
- ✅ `ProductsController` com GET /products
- ✅ `HealthController` com health checks
- ✅ DTOs: CreateOrderRequest, OrderResponse, ProductResponse
- ✅ FluentValidation para validação de entrada
- ✅ Middleware: ExceptionHandling, CorrelationId
- ✅ Swagger/OpenAPI configurado

---

## [0.2.0] - 2026-01-09

### Added - Infrastructure Layer (Fase 3)
- ✅ Entity Framework Core 8.0 com PostgreSQL
- ✅ ApplicationDbContext com auto-timestamps
- ✅ Entity Configurations (Product, Order, OrderItem, Customer)
- ✅ Repository pattern: Repository<T>, ProductRepository, OrderRepository
- ✅ Optimistic Locking via Version field
- ✅ Pessimistic Locking via SELECT FOR UPDATE
- ✅ DependencyInjection extension

---

## [0.1.0] - 2026-01-09

### Added - Setup e Core (Fases 1-2)
- ✅ Estrutura Clean Architecture (.NET 8)
- ✅ Solution com 6 projetos
- ✅ Entidades: Product, Order, OrderItem, Customer, BaseEntity
- ✅ Enums: OrderStatus, StockMovementType
- ✅ Interfaces: IRepository, IOrderRepository, IProductRepository
- ✅ Exceções de domínio
- ✅ Docker Compose (PostgreSQL 16, Redis 7)
- ✅ Documentação de regras (/rules)

---

## Legenda
- ✅ Concluído
- 🔄 Em progresso
- 🔲 Pendente
  - `02-CLEAN-CODE.md` - Padrões Clean Code e SOLID
  - `03-SECURITY.md` - DevSecOps e OWASP
  - `04-GIT-VERSIONING.md` - Git Flow e Conventional Commits
  - `05-CICD.md` - GitHub Actions Pipeline
  - `06-TESTING.md` - Testes Automatizados
  - `07-DOCKER.md` - Containerização
  - `08-DOCUMENTATION.md` - Padrões de Documentação
  - `09-ENGINEERING.md` - Regras de Engenharia
  - `10-CODE-REVIEW.md` - Checklist de Code Review
  - `11-PROGRESS-TRACKING.md` - Regras de Documentação de Progresso

### Infrastructure
- 🔲 Pendente: Setup inicial do projeto .NET 8

---

## Legenda

- ✅ Concluído
- 🔄 Em progresso
- 🔲 Pendente
- ⛔ Bloqueado
