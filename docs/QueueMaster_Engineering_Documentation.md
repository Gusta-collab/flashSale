# 🚀 QueueMaster: Sistema de Vendas de Alta Demanda (Flash Sale)

## Documentação de Engenharia de Software Completa

---

## 📋 Índice

1. [Visão Geral do Projeto](#visão-geral-do-projeto)
2. [Stack Tecnológica](#stack-tecnológica)
3. [Estrutura de Pastas](#estrutura-de-pastas)
4. [Arquitetura do Sistema](#arquitetura-do-sistema)
5. [Arquitetura de Banco de Dados](#arquitetura-de-banco-de-dados)
6. [APIs e Endpoints](#apis-e-endpoints)
7. [Engenharia UTM](#engenharia-utm)
8. [Modelo de Escalabilidade](#modelo-de-escalabilidade)
9. [Padrões e Boas Práticas](#padrões-e-boas-práticas)
10. [Infraestrutura e DevOps](#infraestrutura-e-devops)

---

## 1. Visão Geral do Projeto

### 1.1 Problema
Cenários de alta demanda (Flash Sales, Black Friday, venda de ingressos) geram milhares de requisições simultâneas. Acesso direto ao banco de dados causa:
- **Race Conditions**: Venda de mais itens que o estoque disponível
- **Colapso do Sistema**: Sobrecarga no banco de dados
- **Perda de Dados**: Requisições perdidas durante picos

### 1.2 Solução
Arquitetura de **Buffer e Processamento Assíncrono** utilizando:
- Message Queue (Redis Streams)
- Worker Services desacoplados
- Comunicação Real-time (SignalR)

---

## 2. Stack Tecnológica

### 2.1 Backend
| Componente | Tecnologia | Versão | Justificativa |
|------------|------------|--------|---------------|
| API Principal | .NET 8 Web API | 8.0 | Performance, suporte LTS |
| Worker Service | .NET 8 Worker | 8.0 | Processamento background |
| ORM | Entity Framework Core | 8.0 | Produtividade, migrations |

### 2.2 Mensageria e Cache
| Componente | Tecnologia | Uso |
|------------|------------|-----|
| Message Queue | Redis Streams | Fila de pedidos |
| Cache | Redis | Cache de estoque |
| Distributed Lock | RedLock | Controle de concorrência |

### 2.3 Banco de Dados
| Componente | Tecnologia | Justificativa |
|------------|------------|---------------|
| Principal | PostgreSQL 16 | ACID, performance, extensões |
| Read Replicas | PostgreSQL | Leitura escalável |

### 2.4 Frontend
| Componente | Tecnologia | Versão |
|------------|------------|--------|
| Framework | Next.js | 14+ |
| Real-time | SignalR Client | 8.0 |
| State Management | Zustand | 4.x |

### 2.5 Infraestrutura
| Componente | Tecnologia |
|------------|------------|
| Containerização | Docker |
| Orquestração Local | Docker Compose |
| Orquestração Prod | Kubernetes |
| CI/CD | GitHub Actions |
| Monitoramento | Prometheus + Grafana |
| APM | OpenTelemetry |

---

## 3. Estrutura de Pastas

```
/queuemaster
│
├── /src
│   │
│   ├── /FlashSale.Api                    # API Gateway (Producer)
│   │   ├── /Controllers
│   │   │   ├── OrdersController.cs       # Endpoint de pedidos
│   │   │   ├── ProductsController.cs     # CRUD de produtos
│   │   │   ├── StockController.cs        # Consulta de estoque
│   │   │   └── HealthController.cs       # Health checks
│   │   ├── /Hubs
│   │   │   └── OrderNotificationHub.cs   # SignalR Hub
│   │   ├── /Middleware
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── /DTOs
│   │   │   ├── CreateOrderRequest.cs
│   │   │   ├── OrderStatusResponse.cs
│   │   │   └── ProductResponse.cs
│   │   ├── /Extensions
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   ├── /FlashSale.Worker                 # Consumer Service
│   │   ├── /Consumers
│   │   │   ├── OrderConsumer.cs          # Processa pedidos
│   │   │   └── DeadLetterConsumer.cs     # Processa DLQ
│   │   ├── /Handlers
│   │   │   ├── OrderProcessingHandler.cs
│   │   │   └── NotificationHandler.cs
│   │   ├── Program.cs
│   │   ├── Worker.cs
│   │   └── Dockerfile
│   │
│   ├── /FlashSale.Core                   # Domain Layer
│   │   ├── /Entities
│   │   │   ├── Product.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── Customer.cs
│   │   │   └── StockMovement.cs
│   │   ├── /Enums
│   │   │   ├── OrderStatus.cs
│   │   │   └── PaymentStatus.cs
│   │   ├── /ValueObjects
│   │   │   ├── Money.cs
│   │   │   └── Email.cs
│   │   ├── /Interfaces
│   │   │   ├── IOrderRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   ├── IMessagePublisher.cs
│   │   │   ├── IStockService.cs
│   │   │   └── ICacheService.cs
│   │   ├── /Events
│   │   │   ├── OrderCreatedEvent.cs
│   │   │   ├── OrderProcessedEvent.cs
│   │   │   └── StockUpdatedEvent.cs
│   │   └── /Exceptions
│   │       ├── InsufficientStockException.cs
│   │       └── DuplicateOrderException.cs
│   │
│   ├── /FlashSale.Application            # Application Layer
│   │   ├── /Services
│   │   │   ├── OrderService.cs
│   │   │   ├── StockService.cs
│   │   │   └── NotificationService.cs
│   │   ├── /Commands
│   │   │   ├── CreateOrderCommand.cs
│   │   │   └── ProcessOrderCommand.cs
│   │   ├── /Queries
│   │   │   ├── GetOrderStatusQuery.cs
│   │   │   └── GetProductsQuery.cs
│   │   ├── /Validators
│   │   │   └── CreateOrderValidator.cs
│   │   └── /Mappers
│   │       └── OrderMapper.cs
│   │
│   ├── /FlashSale.Infrastructure         # Infrastructure Layer
│   │   ├── /Data
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── /Configurations
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ProductConfiguration.cs
│   │   │   └── /Migrations
│   │   ├── /Repositories
│   │   │   ├── OrderRepository.cs
│   │   │   └── ProductRepository.cs
│   │   ├── /Redis
│   │   │   ├── RedisStreamPublisher.cs
│   │   │   ├── RedisStreamConsumer.cs
│   │   │   ├── RedisCacheService.cs
│   │   │   └── RedisLockService.cs
│   │   ├── /SignalR
│   │   │   └── SignalRNotificationService.cs
│   │   └── /External
│   │       └── PaymentGatewayService.cs
│   │
│   └── /FlashSale.Tests
│       ├── /Unit
│       │   ├── OrderServiceTests.cs
│       │   └── StockServiceTests.cs
│       ├── /Integration
│       │   ├── OrderFlowTests.cs
│       │   └── RedisStreamTests.cs
│       └── /Load
│           └── k6-load-test.js
│
├── /frontend
│   ├── /app
│   │   ├── /page.tsx                     # Home
│   │   ├── /products
│   │   │   └── page.tsx
│   │   └── /checkout
│   │       └── page.tsx
│   ├── /components
│   │   ├── ProductCard.tsx
│   │   ├── StockCounter.tsx
│   │   └── OrderStatus.tsx
│   ├── /hooks
│   │   └── useSignalR.ts
│   ├── /services
│   │   └── api.ts
│   └── package.json
│
├── /infra
│   ├── /docker
│   │   └── docker-compose.yml
│   ├── /k8s
│   │   ├── api-deployment.yaml
│   │   ├── worker-deployment.yaml
│   │   ├── redis-statefulset.yaml
│   │   └── postgres-statefulset.yaml
│   └── /terraform
│       ├── main.tf
│       └── variables.tf
│
├── /docs
│   ├── architecture.md
│   ├── api-docs.md
│   └── runbook.md
│
├── QueueMaster.sln
├── README.md
└── .gitignore
```

---

## 4. Arquitetura do Sistema

### 4.1 Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                  CLIENTS                                     │
│                    (Browser / Mobile App / External APIs)                    │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            LOAD BALANCER (NGINX/Traefik)                     │
│                              Rate Limiting Layer                             │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    ▼                                   ▼
┌───────────────────────────────┐       ┌───────────────────────────────┐
│        FlashSale.Api          │       │        FlashSale.Api          │
│        (Instance 1)           │       │        (Instance N)           │
│  ┌─────────────────────────┐  │       │  ┌─────────────────────────┐  │
│  │   OrdersController      │  │       │  │   OrdersController      │  │
│  │   - POST /orders        │  │       │  │   - POST /orders        │  │
│  │   - Valida request      │  │       │  │   - Valida request      │  │
│  │   - Publica no Redis    │  │       │  │   - Publica no Redis    │  │
│  │   - Retorna 202         │  │       │  │   - Retorna 202         │  │
│  └─────────────────────────┘  │       │  └─────────────────────────┘  │
│  ┌─────────────────────────┐  │       │  ┌─────────────────────────┐  │
│  │   SignalR Hub           │──┼───────┼──│   SignalR Hub           │  │
│  └─────────────────────────┘  │       │  └─────────────────────────┘  │
└───────────────┬───────────────┘       └───────────────┬───────────────┘
                │                                       │
                └───────────────────┬───────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              REDIS CLUSTER                                   │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         Redis Streams                                │    │
│  │   Stream: orders:pending                                             │    │
│  │   ├── Message 1: {orderId, customerId, productId, qty, timestamp}   │    │
│  │   ├── Message 2: {...}                                               │    │
│  │   └── Message N: {...}                                               │    │
│  │                                                                      │    │
│  │   Stream: orders:dlq (Dead Letter Queue)                            │    │
│  │   └── Failed messages after 3 retries                               │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                           Cache Layer                                │    │
│  │   Key: stock:{productId}  →  Value: quantity (Integer)              │    │
│  │   Key: order:idempotency:{orderId}  →  Value: processed (Boolean)   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                        Distributed Locks                             │    │
│  │   Lock: stock:lock:{productId}  →  Atomic stock operations          │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    ▼                                   ▼
┌───────────────────────────────┐       ┌───────────────────────────────┐
│      FlashSale.Worker         │       │      FlashSale.Worker         │
│        (Instance 1)           │       │        (Instance N)           │
│  ┌─────────────────────────┐  │       │  ┌─────────────────────────┐  │
│  │   Consumer Group        │  │       │  │   Consumer Group        │  │
│  │   - Lê Redis Stream     │  │       │  │   - Lê Redis Stream     │  │
│  │   - Verifica estoque    │  │       │  │   - Verifica estoque    │  │
│  │   - Processa pedido     │  │       │  │   - Processa pedido     │  │
│  │   - Notifica SignalR    │  │       │  │   - Notifica SignalR    │  │
│  └─────────────────────────┘  │       │  └─────────────────────────┘  │
└───────────────┬───────────────┘       └───────────────┬───────────────┘
                │                                       │
                └───────────────────┬───────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           POSTGRESQL CLUSTER                                 │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                          Primary (Write)                             │    │
│  │   - Orders Table                                                     │    │
│  │   - Products Table                                                   │    │
│  │   - Stock Movements Table                                            │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         Read Replicas                                │    │
│  │   - Consultas de produtos                                           │    │
│  │   - Histórico de pedidos                                            │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Fluxo de Dados Detalhado

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Client  │────▶│   API    │────▶│  Redis   │────▶│  Worker  │────▶│ Postgres │
│          │     │          │     │  Stream  │     │          │     │          │
└──────────┘     └──────────┘     └──────────┘     └──────────┘     └──────────┘
     │                │                                   │                │
     │                │                                   │                │
     │    ◀───────────│ 202 Accepted                      │                │
     │    (OrderId)   │                                   │                │
     │                │                                   │                │
     │                │                                   ▼                │
     │                │                          ┌──────────────┐          │
     │                │                          │  Processa    │          │
     │                │                          │  Pedido      │─────────▶│
     │                │                          │  - Valida    │          │
     │                │                          │  - Decrementa│          │
     │                │                          │  - Persiste  │          │
     │                │                          └──────────────┘          │
     │                │                                   │                │
     │                │                                   │                │
     │                │  SignalR Notification             │                │
     │    ◀───────────│◀──────────────────────────────────│                │
     │    (Confirmed/ │                                   │                │
     │     Failed)    │                                   │                │
     ▼                ▼                                   ▼                ▼
```

---

## 5. Arquitetura de Banco de Dados

### 5.1 Modelo Entidade-Relacionamento (ERD)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATABASE SCHEMA                                 │
└─────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────┐          ┌────────────────────────┐
│       customers        │          │       products         │
├────────────────────────┤          ├────────────────────────┤
│ PK  id (UUID)          │          │ PK  id (UUID)          │
│     email (VARCHAR)    │          │     name (VARCHAR)     │
│     name (VARCHAR)     │          │     description (TEXT) │
│     phone (VARCHAR)    │          │     price (DECIMAL)    │
│     created_at (TS)    │          │     stock (INTEGER)    │
│     updated_at (TS)    │          │     max_per_customer   │
└────────────────────────┘          │     is_flash_sale      │
            │                       │     sale_start_at (TS) │
            │                       │     sale_end_at (TS)   │
            │                       │     version (INTEGER)  │◀── Optimistic Lock
            │                       │     created_at (TS)    │
            │                       │     updated_at (TS)    │
            │                       └────────────────────────┘
            │                                   │
            │                                   │
            ▼                                   ▼
┌────────────────────────────────────────────────────────────┐
│                         orders                              │
├────────────────────────────────────────────────────────────┤
│ PK  id (UUID)                                               │
│ FK  customer_id (UUID) ─────────────────────────────────┐   │
│     correlation_id (UUID)  ── Para tracking             │   │
│     status (ENUM)          ── pending/confirmed/failed  │   │
│     total_amount (DECIMAL)                              │   │
│     processed_at (TIMESTAMP)                            │   │
│     failure_reason (TEXT)                               │   │
│     idempotency_key (VARCHAR) ── Unique                 │   │
│     created_at (TIMESTAMP)                              │   │
│     updated_at (TIMESTAMP)                              │   │
└─────────────────────────────────────────────┬──────────────┘
                                              │
                                              ▼
┌────────────────────────────────────────────────────────────┐
│                       order_items                           │
├────────────────────────────────────────────────────────────┤
│ PK  id (UUID)                                               │
│ FK  order_id (UUID) ────────────────────────────────────┐   │
│ FK  product_id (UUID) ──────────────────────────────────┤   │
│     quantity (INTEGER)                                  │   │
│     unit_price (DECIMAL)                                │   │
│     subtotal (DECIMAL)                                  │   │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                    stock_movements                          │
├────────────────────────────────────────────────────────────┤
│ PK  id (UUID)                                               │
│ FK  product_id (UUID)                                       │
│ FK  order_id (UUID) NULLABLE                                │
│     movement_type (ENUM)  ── reserve/confirm/release/adjust │
│     quantity (INTEGER)    ── Pode ser negativo              │
│     previous_stock (INTEGER)                                │
│     new_stock (INTEGER)                                     │
│     created_at (TIMESTAMP)                                  │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                    outbox_messages                          │◀── Transactional Outbox
├────────────────────────────────────────────────────────────┤
│ PK  id (UUID)                                               │
│     event_type (VARCHAR)                                    │
│     payload (JSONB)                                         │
│     processed (BOOLEAN)                                     │
│     processed_at (TIMESTAMP)                                │
│     created_at (TIMESTAMP)                                  │
└────────────────────────────────────────────────────────────┘
```

### 5.2 Scripts SQL

```sql
-- Criação das tabelas principais
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enum para status do pedido
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'confirmed', 'failed', 'cancelled');
CREATE TYPE movement_type AS ENUM ('reserve', 'confirm', 'release', 'adjust');

-- Tabela de Clientes
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Tabela de Produtos
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0 CHECK (stock >= 0),
    max_per_customer INTEGER DEFAULT 1,
    is_flash_sale BOOLEAN DEFAULT FALSE,
    sale_start_at TIMESTAMP WITH TIME ZONE,
    sale_end_at TIMESTAMP WITH TIME ZONE,
    version INTEGER DEFAULT 1,  -- Optimistic Locking
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Tabela de Pedidos
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_id UUID NOT NULL REFERENCES customers(id),
    correlation_id UUID NOT NULL,
    status order_status NOT NULL DEFAULT 'pending',
    total_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    processed_at TIMESTAMP WITH TIME ZONE,
    failure_reason TEXT,
    idempotency_key VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Tabela de Itens do Pedido
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(id),
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10, 2) NOT NULL,
    subtotal DECIMAL(10, 2) NOT NULL
);

-- Tabela de Movimentações de Estoque
CREATE TABLE stock_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES products(id),
    order_id UUID REFERENCES orders(id),
    movement_type movement_type NOT NULL,
    quantity INTEGER NOT NULL,
    previous_stock INTEGER NOT NULL,
    new_stock INTEGER NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Outbox Pattern para eventos
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_type VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    processed BOOLEAN DEFAULT FALSE,
    processed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Índices para performance
CREATE INDEX idx_orders_customer_id ON orders(customer_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_correlation_id ON orders(correlation_id);
CREATE INDEX idx_orders_created_at ON orders(created_at DESC);
CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_order_items_product_id ON order_items(product_id);
CREATE INDEX idx_stock_movements_product_id ON stock_movements(product_id);
CREATE INDEX idx_stock_movements_order_id ON stock_movements(order_id);
CREATE INDEX idx_products_flash_sale ON products(is_flash_sale) WHERE is_flash_sale = TRUE;
CREATE INDEX idx_outbox_unprocessed ON outbox_messages(processed) WHERE processed = FALSE;

-- Função para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers para updated_at
CREATE TRIGGER update_customers_updated_at BEFORE UPDATE ON customers
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_products_updated_at BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_orders_updated_at BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Função para decrementar estoque com lock (Pessimistic)
CREATE OR REPLACE FUNCTION decrement_stock(
    p_product_id UUID,
    p_quantity INTEGER,
    p_order_id UUID
) RETURNS BOOLEAN AS $$
DECLARE
    v_current_stock INTEGER;
    v_new_stock INTEGER;
BEGIN
    -- Lock na linha do produto
    SELECT stock INTO v_current_stock
    FROM products
    WHERE id = p_product_id
    FOR UPDATE;
    
    IF v_current_stock >= p_quantity THEN
        v_new_stock := v_current_stock - p_quantity;
        
        UPDATE products
        SET stock = v_new_stock,
            version = version + 1
        WHERE id = p_product_id;
        
        -- Registra movimentação
        INSERT INTO stock_movements (product_id, order_id, movement_type, quantity, previous_stock, new_stock)
        VALUES (p_product_id, p_order_id, 'confirm', -p_quantity, v_current_stock, v_new_stock);
        
        RETURN TRUE;
    ELSE
        RETURN FALSE;
    END IF;
END;
$$ LANGUAGE plpgsql;
```

### 5.3 Estratégias de Consistência

| Estratégia | Uso | Implementação |
|------------|-----|---------------|
| **Optimistic Locking** | Updates de produtos | Campo `version` incrementado |
| **Pessimistic Locking** | Decremento de estoque | `SELECT FOR UPDATE` |
| **Idempotency Key** | Duplicação de pedidos | Campo `idempotency_key` UNIQUE |
| **Outbox Pattern** | Eventos consistentes | Tabela `outbox_messages` |

---

## 6. APIs e Endpoints

### 6.1 API REST - FlashSale.Api

#### Base URL: `https://api.queuemaster.com/v1`

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/orders` | Cria pedido (enfileira) | JWT |
| GET | `/orders/{id}` | Status do pedido | JWT |
| GET | `/orders/{id}/status` | Status simplificado | JWT |
| GET | `/products` | Lista produtos | Público |
| GET | `/products/{id}` | Detalhe do produto | Público |
| GET | `/products/{id}/stock` | Estoque em tempo real | Público |
| GET | `/health` | Health check | Público |
| GET | `/health/ready` | Readiness probe | Público |

### 6.2 Contratos de API

#### POST /orders - Criar Pedido

**Request:**
```json
{
  "customerId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "quantity": 1
    }
  ],
  "idempotencyKey": "order-123-456-789"
}
```

**Response (202 Accepted):**
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "pending",
  "message": "Seu pedido está sendo processado",
  "estimatedProcessingTime": "5s"
}
```

#### GET /orders/{id}/status

**Response (200 OK):**
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": "confirmed",
  "processedAt": "2024-01-15T10:30:00Z",
  "totalAmount": 299.99
}
```

**Response (200 OK - Failed):**
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": "failed",
  "failureReason": "Estoque insuficiente",
  "processedAt": "2024-01-15T10:30:00Z"
}
```

### 6.3 SignalR Hub

**Hub Path:** `/hubs/orders`

**Server → Client Events:**
| Event | Payload | Descrição |
|-------|---------|-----------|
| `OrderConfirmed` | `{orderId, totalAmount}` | Pedido confirmado |
| `OrderFailed` | `{orderId, reason}` | Pedido falhou |
| `StockUpdated` | `{productId, newStock}` | Estoque atualizado |

**Client → Server Methods:**
| Method | Parameters | Descrição |
|--------|------------|-----------|
| `SubscribeToOrder` | `orderId` | Inscreve para updates |
| `SubscribeToProduct` | `productId` | Inscreve para estoque |

### 6.4 Redis Streams - Estrutura de Mensagens

**Stream:** `orders:pending`
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customerId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "items": [
    {"productId": "550e8400-e29b-41d4-a716-446655440000", "quantity": 1}
  ],
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "idempotencyKey": "order-123-456-789",
  "timestamp": "2024-01-15T10:30:00Z",
  "retryCount": 0
}
```

**Consumer Group:** `order-processors`

---

## 7. Engenharia UTM

### 7.1 Universal Tracking Module - Estratégia

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           UTM TRACKING FLOW                                  │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Campaign   │────▶│   Landing    │────▶│   Product    │────▶│   Checkout   │
│   Source     │     │   Page       │     │   Page       │     │   Page       │
│              │     │              │     │              │     │              │
│ utm_source   │     │ Capture UTM  │     │ Persist UTM  │     │ Send UTM     │
│ utm_medium   │     │ to Session   │     │ in Session   │     │ with Order   │
│ utm_campaign │     │              │     │              │     │              │
│ utm_content  │     │              │     │              │     │              │
│ utm_term     │     │              │     │              │     │              │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

### 7.2 Parâmetros UTM Suportados

| Parâmetro | Descrição | Exemplo |
|-----------|-----------|---------|
| `utm_source` | Origem do tráfego | google, facebook, newsletter |
| `utm_medium` | Tipo de mídia | cpc, banner, email |
| `utm_campaign` | Nome da campanha | black_friday_2024 |
| `utm_content` | Variação do anúncio | banner_v1, link_rodape |
| `utm_term` | Palavra-chave (PPC) | ingressos_show |

### 7.3 Estrutura de Dados UTM

```sql
-- Tabela para armazenar dados UTM
CREATE TABLE utm_tracking (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    session_id VARCHAR(255) NOT NULL,
    order_id UUID REFERENCES orders(id),
    utm_source VARCHAR(100),
    utm_medium VARCHAR(100),
    utm_campaign VARCHAR(255),
    utm_content VARCHAR(255),
    utm_term VARCHAR(255),
    landing_page VARCHAR(500),
    referrer VARCHAR(500),
    user_agent TEXT,
    ip_address INET,
    country_code VARCHAR(2),
    city VARCHAR(100),
    device_type VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_utm_tracking_session ON utm_tracking(session_id);
CREATE INDEX idx_utm_tracking_order ON utm_tracking(order_id);
CREATE INDEX idx_utm_tracking_source ON utm_tracking(utm_source);
CREATE INDEX idx_utm_tracking_campaign ON utm_tracking(utm_campaign);
CREATE INDEX idx_utm_tracking_created ON utm_tracking(created_at);
```

### 7.4 Implementação Frontend

```typescript
// hooks/useUTM.ts
import { useSearchParams } from 'next/navigation';
import { useEffect } from 'react';

interface UTMParams {
  utm_source?: string;
  utm_medium?: string;
  utm_campaign?: string;
  utm_content?: string;
  utm_term?: string;
}

export function useUTM() {
  const searchParams = useSearchParams();
  
  useEffect(() => {
    const utmParams: UTMParams = {
      utm_source: searchParams.get('utm_source') || undefined,
      utm_medium: searchParams.get('utm_medium') || undefined,
      utm_campaign: searchParams.get('utm_campaign') || undefined,
      utm_content: searchParams.get('utm_content') || undefined,
      utm_term: searchParams.get('utm_term') || undefined,
    };
    
    // Persistir no sessionStorage
    if (Object.values(utmParams).some(v => v)) {
      sessionStorage.setItem('utm_params', JSON.stringify(utmParams));
      sessionStorage.setItem('landing_page', window.location.pathname);
      sessionStorage.setItem('referrer', document.referrer);
    }
  }, [searchParams]);
  
  const getUTMParams = (): UTMParams => {
    const stored = sessionStorage.getItem('utm_params');
    return stored ? JSON.parse(stored) : {};
  };
  
  return { getUTMParams };
}
```

### 7.5 Relatórios de Conversão

```sql
-- View para análise de conversões por campanha
CREATE VIEW utm_conversion_report AS
SELECT 
    ut.utm_source,
    ut.utm_medium,
    ut.utm_campaign,
    COUNT(DISTINCT ut.session_id) as total_sessions,
    COUNT(DISTINCT ut.order_id) as total_orders,
    ROUND(COUNT(DISTINCT ut.order_id)::DECIMAL / 
          NULLIF(COUNT(DISTINCT ut.session_id), 0) * 100, 2) as conversion_rate,
    SUM(o.total_amount) as total_revenue,
    AVG(o.total_amount) as avg_order_value
FROM utm_tracking ut
LEFT JOIN orders o ON ut.order_id = o.id AND o.status = 'confirmed'
GROUP BY ut.utm_source, ut.utm_medium, ut.utm_campaign
ORDER BY total_revenue DESC;
```

---

## 8. Modelo de Escalabilidade

### 8.1 Estratégia de Escala Horizontal

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        SCALING ARCHITECTURE                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                            ┌─────────────────────┐
                            │   Load Balancer     │
                            │   (NGINX/Traefik)   │
                            │   - Round Robin     │
                            │   - Health Checks   │
                            └──────────┬──────────┘
                                       │
              ┌────────────────────────┼────────────────────────┐
              │                        │                        │
              ▼                        ▼                        ▼
     ┌────────────────┐       ┌────────────────┐       ┌────────────────┐
     │    API Pod 1   │       │    API Pod 2   │       │    API Pod N   │
     │   (Stateless)  │       │   (Stateless)  │       │   (Stateless)  │
     └────────────────┘       └────────────────┘       └────────────────┘
              │                        │                        │
              └────────────────────────┼────────────────────────┘
                                       │
                                       ▼
                            ┌─────────────────────┐
                            │   Redis Cluster     │
                            │   - 3 Masters       │
                            │   - 3 Replicas      │
                            │   - Sharding        │
                            └──────────┬──────────┘
                                       │
              ┌────────────────────────┼────────────────────────┐
              │                        │                        │
              ▼                        ▼                        ▼
     ┌────────────────┐       ┌────────────────┐       ┌────────────────┐
     │  Worker Pod 1  │       │  Worker Pod 2  │       │  Worker Pod N  │
     │ Consumer Group │       │ Consumer Group │       │ Consumer Group │
     └────────────────┘       └────────────────┘       └────────────────┘
              │                        │                        │
              └────────────────────────┼────────────────────────┘
                                       │
                                       ▼
                            ┌─────────────────────┐
                            │  PostgreSQL Cluster │
                            │   - Primary (Write) │
                            │   - Read Replicas   │
                            │   - PgBouncer       │
                            └─────────────────────┘
```

### 8.2 Configuração de Auto-Scaling

```yaml
# k8s/api-hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: flashsale-api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: flashsale-api
  minReplicas: 3
  maxReplicas: 50
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
    - type: Pods
      pods:
        metric:
          name: http_requests_per_second
        target:
          type: AverageValue
          averageValue: 1000
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
        - type: Percent
          value: 100
          periodSeconds: 15
        - type: Pods
          value: 10
          periodSeconds: 15
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
        - type: Percent
          value: 10
          periodSeconds: 60
```

### 8.3 Métricas de Capacidade

| Componente | Métrica | Capacidade Esperada |
|------------|---------|---------------------|
| API (por pod) | Requests/segundo | 2,000 - 5,000 |
| Worker (por pod) | Mensagens/segundo | 500 - 1,000 |
| Redis | Operações/segundo | 100,000+ |
| PostgreSQL | Transactions/segundo | 5,000 - 10,000 |

### 8.4 Estratégias de Cache

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CACHE LAYERS                                       │
└─────────────────────────────────────────────────────────────────────────────┘

┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│   Browser     │     │   CDN Edge    │     │   Redis       │
│   Cache       │     │   Cache       │     │   Cache       │
│ (5 min TTL)   │     │ (1 min TTL)   │     │ (30s TTL)     │
└───────────────┘     └───────────────┘     └───────────────┘
       │                     │                     │
       │  Cache-Control      │  Cloudflare/        │  Stock, Session
       │  Headers            │  Fastly             │  Product Details
       ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│                     CACHE INVALIDATION                       │
├─────────────────────────────────────────────────────────────┤
│  Event: StockUpdated → Invalidate product:{id}:stock        │
│  Event: ProductChanged → Invalidate product:{id}            │
│  Event: FlashSaleStarted → Warm cache with product data     │
└─────────────────────────────────────────────────────────────┘
```

### 8.5 Limites e Proteções

| Proteção | Implementação | Configuração |
|----------|---------------|--------------|
| Rate Limiting | Sliding Window | 100 req/min por IP |
| Circuit Breaker | Polly (.NET) | 50% falha = abrir |
| Backpressure | Redis Stream | Max 100k mensagens |
| Connection Pool | PgBouncer | 100 conexões |
| Request Timeout | Middleware | 30 segundos |

---

## 9. Padrões e Boas Práticas

### 9.1 Padrões Implementados

| Padrão | Uso | Benefício |
|--------|-----|-----------|
| **CQRS** | Separar leitura/escrita | Performance otimizada |
| **Event Sourcing** | Stock movements | Auditoria completa |
| **Saga Pattern** | Pedidos distribuídos | Consistência eventual |
| **Outbox Pattern** | Eventos confiáveis | Zero mensagens perdidas |
| **Circuit Breaker** | Chamadas externas | Resiliência |
| **Retry Policy** | Operações falhas | Recuperação automática |
| **Bulkhead** | Isolamento | Falhas contidas |

### 9.2 Tratamento de Race Condition

```csharp
// Implementação com Optimistic Concurrency
public async Task<bool> DecrementStockAsync(Guid productId, int quantity, CancellationToken ct)
{
    const int maxRetries = 3;
    
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, ct);
        
        if (product == null || product.Stock < quantity)
            return false;
        
        product.Stock -= quantity;
        
        try
        {
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Reload and retry
            await _dbContext.Entry(product).ReloadAsync(ct);
        }
    }
    
    return false;
}
```

### 9.3 Idempotência

```csharp
public async Task<OrderResult> ProcessOrderAsync(OrderMessage message, CancellationToken ct)
{
    // Verificar se já foi processado
    var idempotencyKey = $"order:processed:{message.IdempotencyKey}";
    var alreadyProcessed = await _redis.StringGetAsync(idempotencyKey);
    
    if (alreadyProcessed.HasValue)
    {
        _logger.LogInformation("Order {OrderId} already processed, skipping", message.OrderId);
        return new OrderResult { AlreadyProcessed = true };
    }
    
    // Processar pedido...
    
    // Marcar como processado (TTL 24h)
    await _redis.StringSetAsync(idempotencyKey, "1", TimeSpan.FromHours(24));
    
    return new OrderResult { Success = true };
}
```

### 9.4 Dead Letter Queue

```csharp
public async Task HandleMessageFailure(OrderMessage message, Exception exception)
{
    message.RetryCount++;
    
    if (message.RetryCount >= _maxRetries)
    {
        // Mover para DLQ
        await _redis.StreamAddAsync("orders:dlq", new NameValueEntry[]
        {
            new("payload", JsonSerializer.Serialize(message)),
            new("error", exception.Message),
            new("failedAt", DateTime.UtcNow.ToString("O"))
        });
        
        _logger.LogError(exception, "Order {OrderId} moved to DLQ after {Retries} retries", 
            message.OrderId, _maxRetries);
    }
    else
    {
        // Requeue com backoff exponencial
        var delay = TimeSpan.FromSeconds(Math.Pow(2, message.RetryCount));
        await Task.Delay(delay);
        await _publisher.PublishAsync(message);
    }
}
```

---

## 10. Infraestrutura e DevOps

### 10.1 Docker Compose (Desenvolvimento)

```yaml
version: '3.8'

services:
  api:
    build:
      context: ./src/FlashSale.Api
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=flashsale;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
    networks:
      - flashsale-network

  worker:
    build:
      context: ./src/FlashSale.Worker
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=flashsale;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
    deploy:
      replicas: 3
    networks:
      - flashsale-network

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: flashsale
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./infra/sql/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    networks:
      - flashsale-network

  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes
    volumes:
      - redis_data:/data
    ports:
      - "6379:6379"
    networks:
      - flashsale-network

  prometheus:
    image: prom/prometheus:latest
    volumes:
      - ./infra/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
    ports:
      - "9090:9090"
    networks:
      - flashsale-network

  grafana:
    image: grafana/grafana:latest
    volumes:
      - grafana_data:/var/lib/grafana
    ports:
      - "3000:3000"
    networks:
      - flashsale-network

volumes:
  postgres_data:
  redis_data:
  grafana_data:

networks:
  flashsale-network:
    driver: bridge
```

### 10.2 CI/CD Pipeline (GitHub Actions)

```yaml
name: QueueMaster CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Unit Tests
        run: dotnet test --no-build --filter Category=Unit
      
      - name: Integration Tests
        run: |
          docker-compose -f docker-compose.test.yml up -d
          dotnet test --no-build --filter Category=Integration
          docker-compose -f docker-compose.test.yml down

  load-test:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup k6
        uses: grafana/k6-action@v0.3.1
      
      - name: Run Load Tests
        run: k6 run ./src/FlashSale.Tests/Load/k6-load-test.js

  deploy:
    needs: [build-and-test, load-test]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      
      - name: Build and Push Docker Images
        run: |
          echo ${{ secrets.GITHUB_TOKEN }} | docker login ghcr.io -u ${{ github.actor }} --password-stdin
          docker build -t ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:${{ github.sha }} ./src/FlashSale.Api
          docker build -t ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/worker:${{ github.sha }} ./src/FlashSale.Worker
          docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:${{ github.sha }}
          docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/worker:${{ github.sha }}
      
      - name: Deploy to Kubernetes
        uses: azure/k8s-deploy@v4
        with:
          manifests: ./infra/k8s/
          images: |
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:${{ github.sha }}
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/worker:${{ github.sha }}
```

### 10.3 Script de Teste de Carga (k6)

```javascript
// k6-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

const errorRate = new Rate('errors');
const orderLatency = new Trend('order_latency');

export const options = {
  scenarios: {
    flash_sale_spike: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 100 },   // Ramp up
        { duration: '30s', target: 5000 },  // Flash sale peak
        { duration: '10s', target: 5000 },  // Sustain
        { duration: '10s', target: 0 },     // Ramp down
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% under 500ms
    errors: ['rate<0.01'],              // Error rate < 1%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  const productId = '550e8400-e29b-41d4-a716-446655440000';
  
  const payload = JSON.stringify({
    customerId: `customer-${__VU}`,
    items: [{ productId: productId, quantity: 1 }],
    idempotencyKey: `order-${__VU}-${Date.now()}`
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const start = Date.now();
  const res = http.post(`${BASE_URL}/api/v1/orders`, payload, params);
  const latency = Date.now() - start;

  orderLatency.add(latency);

  const success = check(res, {
    'status is 202': (r) => r.status === 202,
    'has orderId': (r) => JSON.parse(r.body).orderId !== undefined,
  });

  errorRate.add(!success);
  
  sleep(0.1);
}
```

---

## 📊 Resumo Executivo

| Aspecto | Decisão | Justificativa |
|---------|---------|---------------|
| **Linguagem** | C# (.NET 8) | Performance, tipagem forte, suporte enterprise |
| **Arquitetura** | Microservices (API + Worker) | Escalabilidade independente |
| **Mensageria** | Redis Streams | Baixa latência, persistência, consumer groups |
| **Banco** | PostgreSQL | ACID, extensões, maturidade |
| **Real-time** | SignalR | Integração nativa .NET, WebSocket fallback |
| **Escala** | Kubernetes + HPA | Auto-scaling baseado em métricas |

---

## 🎯 Próximos Passos

1. [ ] Implementar código base do projeto
2. [ ] Configurar ambiente Docker local
3. [ ] Implementar testes unitários
4. [ ] Criar testes de integração
5. [ ] Executar testes de carga com k6
6. [ ] Documentar resultados para LinkedIn

---

**Autor:** QueueMaster Team  
**Versão:** 1.0.0  
**Última Atualização:** Janeiro 2026
