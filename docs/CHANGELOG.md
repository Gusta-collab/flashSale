# Changelog

Todas as mudanças notáveis do projeto QueueMaster serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).
Este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [Unreleased]

### Added
- ✅ Estrutura de projeto Clean Architecture (.NET 8)
- ✅ Solution com 6 projetos (Api, Worker, Core, Application, Infrastructure, Tests)
- ✅ Entidades de domínio: Product, Order, OrderItem, Customer, BaseEntity
- ✅ Enums: OrderStatus, StockMovementType
- ✅ Interfaces: IRepository, IOrderRepository, IProductRepository
- ✅ Exceções de domínio: DomainException, InsufficientStockException, etc.
- ✅ Docker Compose com PostgreSQL 16 e Redis 7
- ✅ Arquivos de configuração: .env.example, .gitignore

### Infrastructure
- 🔄 Em progresso: Entity Framework Core setup
  - `00-INDEX.md` - Índice geral
  - `01-PROGRESS.md` - Milestones e KPIs
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
