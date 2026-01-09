# 🏗️ Arquitetura do Sistema QueueMaster

## Sistema de Vendas de Alta Demanda (Flash Sale)

---

## 1. Visão Geral

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                CLIENTES                                      │
│                         (Browser / Mobile / API)                             │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            LOAD BALANCER                                     │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    ▼                                   ▼
              ┌──────────┐                        ┌──────────┐
              │   API    │                        │   API    │
              │ (Pod 1)  │                        │ (Pod N)  │
              └────┬─────┘                        └────┬─────┘
                   │                                   │
                   └─────────────┬─────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                             REDIS CLUSTER                                    │
│                                                                             │
│   ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐         │
│   │  Redis Streams  │    │     Cache       │    │  Distributed    │         │
│   │ orders:pending  │    │  stock:{id}     │    │     Locks       │         │
│   └─────────────────┘    └─────────────────┘    └─────────────────┘         │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    ▼                                   ▼
              ┌──────────┐                        ┌──────────┐
              │  Worker  │                        │  Worker  │
              │ (Pod 1)  │                        │ (Pod N)  │
              └────┬─────┘                        └────┬─────┘
                   │                                   │
                   └─────────────┬─────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          POSTGRESQL CLUSTER                                  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │  PRIMARY (Write)                                                     │   │
│   │  - orders, products, customers, stock_movements                     │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │  READ REPLICAS                                                       │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Componentes

### 2.1 FlashSale.Api
- **Função**: Gateway de entrada
- **Responsabilidades**:
  - Receber requisições HTTP
  - Validar input
  - Publicar no Redis Stream
  - Retornar 202 Accepted
  - Hub SignalR para real-time

### 2.2 FlashSale.Worker
- **Função**: Processador de fila
- **Responsabilidades**:
  - Consumir Redis Stream
  - Verificar estoque
  - Processar transações
  - Notificar via SignalR

### 2.3 FlashSale.Core
- **Função**: Domínio da aplicação
- **Responsabilidades**:
  - Entidades de negócio
  - Interfaces (contratos)
  - Regras de domínio
  - Zero dependências externas

### 2.4 FlashSale.Infrastructure
- **Função**: Implementações concretas
- **Responsabilidades**:
  - Repositórios (EF Core)
  - Redis Client
  - SignalR Service

---

## 3. Fluxo de Dados

```
1. Cliente clica "Comprar"
   │
   ▼
2. API recebe POST /orders
   │
   ├── Valida request
   ├── Publica no Redis Stream
   └── Retorna 202 Accepted
   │
   ▼
3. Worker consome mensagem
   │
   ├── Verifica idempotência
   ├── Abre transação PostgreSQL
   ├── SELECT stock FOR UPDATE
   ├── Decrementa estoque
   ├── INSERT order
   ├── COMMIT
   └── ACK mensagem
   │
   ▼
4. Notifica cliente via SignalR
   │
   └── "Pedido confirmado!" ✅
```

---

## 4. Stack Tecnológica

| Camada | Tecnologia |
|--------|------------|
| Frontend | Next.js 14 |
| API | .NET 8 Web API |
| Worker | .NET 8 Worker Service |
| Real-time | SignalR |
| Message Queue | Redis Streams |
| Cache | Redis |
| Database | PostgreSQL 16 |
| Container | Docker |
| Orchestration | Kubernetes |
| CI/CD | GitHub Actions |

---

## 5. Decisões de Arquitetura

| Decisão | Documento |
|---------|-----------|
| Redis Streams | [ADR-001](./decisions/ADR-001-redis-streams.md) |
| PostgreSQL | [ADR-002](./decisions/ADR-002-postgresql.md) |
| .NET 8 | [ADR-003](./decisions/ADR-003-dotnet8.md) |

---

## 6. Escalabilidade

- **API**: Horizontal (N pods, stateless)
- **Worker**: Horizontal (Consumer Groups)
- **Redis**: Cluster mode (3 masters + 3 replicas)
- **PostgreSQL**: Read replicas + PgBouncer

---

📅 **Última atualização**: 2026-01-09
