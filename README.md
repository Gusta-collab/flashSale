# 🎫 FlashSale - Sistema de Venda de Ingressos de Alta Demanda

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Sistema de **Flash Sale para Ingressos** desenvolvido com arquitetura **Clean Architecture**, processamento assíncrono via **Redis Streams**, e notificações em tempo real com **SignalR**. Ideal para cenários de alta demanda como venda de ingressos de shows, festivais e eventos.

---

## 📋 Sumário

- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Endpoints](#-endpoints)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Documentação](#-documentação)

---

## 🏗 Arquitetura

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Client    │────▶│    API      │────▶│   Redis     │
│  (Browser)  │◀────│  (SignalR)  │     │  Streams    │
└─────────────┘     └─────────────┘     └──────┬──────┘
                           │                   │
                           │                   ▼
                    ┌──────▼──────┐     ┌─────────────┐
                    │ PostgreSQL  │◀────│   Worker    │
                    │     16      │     │  Service    │
                    └─────────────┘     └─────────────┘
```

### Fluxo do Pedido
1. **API** recebe pedido (POST /api/v1/orders)
2. Publica no **Redis Stream** `orders:pending`
3. **Worker** consome com Consumer Groups
4. Valida estoque, decrementa, confirma
5. **SignalR** notifica cliente em tempo real

---

## 🛠 Tecnologias

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| .NET | 8.0 | Backend |
| PostgreSQL | 16 | Banco de dados |
| Redis | 7 | Cache + Message Queue |
| SignalR | - | WebSocket (tempo real) |
| Entity Framework Core | 8.0 | ORM |
| FluentValidation | 11.3 | Validação |

---

## 📦 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) ou instalação local de PostgreSQL e Redis

---

## 🚀 Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/Gusta-collab/flashSale.git
cd flashSale
```

### 2. Suba o ambiente com Docker

```bash
docker-compose up -d
```

### 3. Execute as migrações

```bash
dotnet ef database update --project src/FlashSale.Infrastructure/FlashSale.Infrastructure
```

### 4. Execute a API

```bash
dotnet run --project src/FlashSale.Api/FlashSale.Api
```

### 5. Execute o Worker (outra janela)

```bash
dotnet run --project src/FlashSale.Worker/FlashSale.Worker
```

---

## 📡 Endpoints

### API REST

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/orders` | Criar pedido |
| GET | `/api/v1/orders/{id}` | Buscar pedido |
| GET | `/api/v1/orders/{id}/status` | Status do pedido |
| GET | `/api/v1/products` | Listar produtos |
| GET | `/api/v1/products/{id}` | Buscar produto |
| GET | `/health` | Health check |
| GET | `/health/ready` | Readiness check |

### SignalR

| Hub | Endpoint |
|-----|----------|
| Orders | `/hubs/orders` |

**Eventos:**
- `OrderConfirmed` - Pedido confirmado
- `OrderFailed` - Pedido falhou
- `OrderStatusChanged` - Status alterado

---

## 📂 Estrutura do Projeto

```
flashSale/
├── src/
│   ├── FlashSale.Core/           # Entidades, Interfaces
│   ├── FlashSale.Application/    # Casos de uso
│   ├── FlashSale.Infrastructure/ # EF Core, Redis
│   ├── FlashSale.Api/            # Controllers, Hubs
│   └── FlashSale.Worker/         # Background Service
├── tests/
│   └── FlashSale.Tests/          # Testes
├── docs/                          # Documentação
├── rules/                         # Regras de desenvolvimento
└── docker-compose.yml             # Ambiente local
```

---

## 📚 Documentação

- [Regras de Desenvolvimento](./rules/00-INDEX.md)
- [CHANGELOG](./docs/CHANGELOG.md)
- [Arquitetura](./docs/architecture.md)
- [Planejamento de Tasks](./docs/TASKS.md)
- [Plano de Integração](./docs/INTEGRATION_PLAN.md)

### ADRs (Architecture Decision Records)
- [ADR-001: Redis Streams](./docs/decisions/ADR-001-redis-streams.md)
- [ADR-002: PostgreSQL](./docs/decisions/ADR-002-postgresql.md)
- [ADR-003: .NET 8](./docs/decisions/ADR-003-dotnet8.md)

---

## 📄 Licença

Este projeto está sob a licença MIT.

---

**Desenvolvido com ❤️ seguindo Clean Architecture e boas práticas**