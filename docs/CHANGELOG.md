# Changelog

Todas as mudanças notáveis do projeto QueueMaster serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).
Este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

### Added
- 🔄 Redis Streams integration (em andamento)

---

## [0.3.0] - 2026-01-09

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
