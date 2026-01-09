# 📚 QueueMaster: Explicação Detalhada da Arquitetura

## Guia Completo para Entender Cada Componente do Sistema

---

## 📖 Índice

1. [Entendendo o Problema](#1-entendendo-o-problema)
2. [Por Que Essa Stack?](#2-por-que-essa-stack)
3. [Estrutura de Pastas Explicada](#3-estrutura-de-pastas-explicada)
4. [Arquitetura do Sistema](#4-arquitetura-do-sistema)
5. [Banco de Dados](#5-banco-de-dados)
6. [APIs e Comunicação](#6-apis-e-comunicação)
7. [Engenharia UTM](#7-engenharia-utm)
8. [Escalabilidade](#8-escalabilidade)
9. [Padrões de Design](#9-padrões-de-design)
10. [DevOps e Infraestrutura](#10-devops-e-infraestrutura)

---

## 1. Entendendo o Problema

### 1.1 O Cenário Real

Imagine a seguinte situação:

```
🎫 Venda de Ingressos para um Show Internacional
   - 10.000 ingressos disponíveis
   - 500.000 pessoas querendo comprar
   - Venda abre às 10:00 da manhã
   - Em 1 segundo: 50.000 requisições simultâneas
```

### 1.2 O Que Acontece em um Sistema Tradicional?

```
Sistema Tradicional (Ruim):

Cliente → API → Banco de Dados → Resposta
   ↓
[50.000 conexões simultâneas ao banco]
   ↓
💥 CRASH! Banco de dados não aguenta
```

**Problemas:**
1. **Sobrecarga do Banco**: PostgreSQL não foi feito para 50k conexões simultâneas
2. **Race Condition**: 100 pessoas leem "1 ingresso disponível" ao mesmo tempo, todas compram, estoque fica -99
3. **Timeout**: Requisições demoram tanto que expiram
4. **Perda de Vendas**: Sistema cai, empresa perde dinheiro

### 1.3 Nossa Solução: Buffer Assíncrono

```
Sistema QueueMaster (Bom):

Cliente → API → [Redis Stream] → Worker → Banco de Dados
   ↓           (Buffer rápido)     ↓
 "202 OK"                      Processa 
 "Na fila!"                    1 por vez
                                  ↓
                              Notifica via
                              WebSocket ✅
```

**Por que funciona:**
- Redis aguenta **100.000+ operações/segundo**
- Worker processa **sequencialmente** = sem race condition
- Cliente não espera = experiência boa
- Banco recebe carga controlada

---

## 2. Por Que Essa Stack?

### 2.1 Tabela de Decisões

| Tecnologia | Alternativas | Por que escolhemos |
|------------|--------------|-------------------|
| **.NET 8** | Node.js, Go, Java | Performance C#, tipagem forte, suporte empresarial |
| **Redis Streams** | RabbitMQ, Kafka, SQS | Latência ultra-baixa, já usamos Redis para cache |
| **PostgreSQL** | MySQL, MongoDB, SQL Server | ACID forte, extensões, gratuito |
| **SignalR** | Socket.io, Pusher | Integração nativa .NET, fallback automático |
| **Docker** | VMs, Bare metal | Portabilidade, isolamento, CI/CD fácil |

### 2.2 Explicação de Cada Tecnologia

#### .NET 8 (C#)

```csharp
// Por que C#?
// 1. Tipagem forte = menos bugs em produção
public class Order
{
    public Guid Id { get; set; }           // Não pode ser null por acidente
    public decimal TotalAmount { get; set; } // Precisão monetária
    public OrderStatus Status { get; set; }  // Enum tipado
}

// 2. Async/Await nativo = alta concorrência
public async Task<Order> ProcessOrderAsync(Guid orderId)
{
    // Não bloqueia a thread enquanto espera o banco
    return await _repository.GetByIdAsync(orderId);
}

// 3. Dependency Injection nativo = código testável
public class OrderService
{
    private readonly IOrderRepository _repository; // Interface, não implementação
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository; // Injetado pelo container
    }
}
```

#### Redis Streams

```
Redis Streams é como uma "fila de mensagens" dentro do Redis.

Diferente do Redis List (LPUSH/RPOP):
- ✅ Persistência garantida
- ✅ Consumer Groups (vários workers)
- ✅ Acknowledgment (confirma processamento)
- ✅ Replay de mensagens (se falhar, tenta de novo)

Estrutura:
┌─────────────────────────────────────────────────┐
│ Stream: orders:pending                          │
├─────────────────────────────────────────────────┤
│ ID: 1704067200000-0                             │
│ Data: {orderId: "abc", productId: "xyz", qty: 1}│
├─────────────────────────────────────────────────┤
│ ID: 1704067200001-0                             │
│ Data: {orderId: "def", productId: "xyz", qty: 2}│
└─────────────────────────────────────────────────┘
```

#### PostgreSQL

```sql
-- Por que PostgreSQL?

-- 1. ACID (Atomicidade, Consistência, Isolamento, Durabilidade)
BEGIN;
  UPDATE products SET stock = stock - 1 WHERE id = 'xyz';
  INSERT INTO orders (product_id, quantity) VALUES ('xyz', 1);
COMMIT;
-- Se qualquer comando falhar, TUDO é revertido

-- 2. SELECT FOR UPDATE (Lock de linha)
SELECT stock FROM products WHERE id = 'xyz' FOR UPDATE;
-- Ninguém mais pode ler essa linha até eu terminar

-- 3. Extensões incríveis
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";  -- UUIDs nativos
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements"; -- Análise de queries
```

---

## 3. Estrutura de Pastas Explicada

### 3.1 Visão Geral (Clean Architecture)

```
A "Clean Architecture" organiza o código em camadas:

┌─────────────────────────────────────────────────────────────┐
│                    APRESENTAÇÃO (API)                        │
│         Controllers, DTOs, Middleware, Hubs                  │
├─────────────────────────────────────────────────────────────┤
│                    APLICAÇÃO                                 │
│         Services, Commands, Queries, Validators              │
├─────────────────────────────────────────────────────────────┤
│                    DOMÍNIO (Core)                            │
│         Entities, Interfaces, ValueObjects, Events           │
├─────────────────────────────────────────────────────────────┤
│                    INFRAESTRUTURA                            │
│         Repositories, Redis, Database, External APIs         │
└─────────────────────────────────────────────────────────────┘

Regra de Ouro: Camadas internas NÃO conhecem camadas externas
- Core não sabe que PostgreSQL existe
- Core define IOrderRepository (interface)
- Infra implementa OrderRepository (classe concreta)
```

### 3.2 Cada Pasta em Detalhe

#### `/src/FlashSale.Api` - A Porta de Entrada

```
FlashSale.Api/
├── Controllers/          ← Recebe requisições HTTP
│   └── OrdersController.cs
│       // POST /orders → Valida → Enfileira → Retorna 202
│
├── Hubs/                 ← WebSocket (SignalR)
│   └── OrderNotificationHub.cs
│       // Envia "Pedido confirmado!" para o navegador
│
├── Middleware/           ← Intercepta todas as requisições
│   ├── RateLimitingMiddleware.cs
│   │   // Bloqueia se > 100 req/min do mesmo IP
│   │
│   ├── CorrelationIdMiddleware.cs
│   │   // Adiciona ID único para rastrear logs
│   │
│   └── ExceptionHandlingMiddleware.cs
│       // Captura erros e retorna JSON bonito
│
├── DTOs/                 ← Data Transfer Objects
│   ├── CreateOrderRequest.cs
│   │   // { customerId, items[], idempotencyKey }
│   │
│   └── OrderStatusResponse.cs
│       // { orderId, status, processedAt }
│
└── Program.cs            ← Configuração da aplicação
```

**Exemplo de Controller:**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // 1. Validar request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // 2. Criar mensagem para a fila
        var message = new OrderMessage
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            CorrelationId = HttpContext.GetCorrelationId()
        };
        
        // 3. Publicar no Redis Stream (microssegundos!)
        await _publisher.PublishAsync("orders:pending", message);
        
        // 4. Retornar 202 Accepted (não 200 OK!)
        return Accepted(new { 
            orderId = message.OrderId,
            message = "Seu pedido está sendo processado"
        });
    }
}
```

#### `/src/FlashSale.Worker` - O Processador

```
FlashSale.Worker/
├── Consumers/            ← Lê mensagens da fila
│   ├── OrderConsumer.cs
│   │   // Loop infinito: Lê Redis → Processa → ACK
│   │
│   └── DeadLetterConsumer.cs
│       // Processa mensagens que falharam 3x
│
├── Handlers/             ← Lógica de processamento
│   ├── OrderProcessingHandler.cs
│   │   // Verifica estoque → Decrementa → Salva pedido
│   │
│   └── NotificationHandler.cs
│       // Envia SignalR para o cliente
│
└── Worker.cs             ← BackgroundService do .NET
```

**Exemplo de Consumer:**

```csharp
public class OrderConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Loop infinito enquanto a aplicação roda
        while (!ct.IsCancellationRequested)
        {
            // Lê próxima mensagem do Redis Stream
            var message = await _redis.StreamReadGroupAsync(
                "orders:pending",      // Nome do stream
                "order-processors",    // Consumer group
                _consumerId,           // ID deste worker
                ">",                   // Apenas mensagens novas
                count: 1
            );
            
            if (message != null)
            {
                try
                {
                    // Processa o pedido
                    await _handler.ProcessAsync(message);
                    
                    // Confirma que processou (ACK)
                    await _redis.StreamAcknowledgeAsync("orders:pending", "order-processors", message.Id);
                }
                catch (Exception ex)
                {
                    // Não dá ACK = mensagem volta para a fila
                    _logger.LogError(ex, "Falha ao processar pedido");
                }
            }
        }
    }
}
```

#### `/src/FlashSale.Core` - O Coração do Sistema

```
FlashSale.Core/
├── Entities/             ← Objetos de negócio
│   ├── Product.cs
│   │   public class Product
│   │   {
│   │       public Guid Id { get; set; }
│   │       public string Name { get; set; }
│   │       public decimal Price { get; set; }
│   │       public int Stock { get; set; }
│   │       public int Version { get; set; }  // ← Optimistic Lock
│   │   }
│   │
│   └── Order.cs
│
├── Interfaces/           ← Contratos (não implementações!)
│   ├── IOrderRepository.cs
│   │   public interface IOrderRepository
│   │   {
│   │       Task<Order> GetByIdAsync(Guid id);
│   │       Task CreateAsync(Order order);
│   │   }
│   │   // Quem implementa? FlashSale.Infrastructure
│   │
│   └── IMessagePublisher.cs
│
├── ValueObjects/         ← Objetos imutáveis
│   └── Money.cs
│       public record Money(decimal Amount, string Currency);
│       // Imutável: new Money(100, "BRL")
│
└── Exceptions/           ← Exceções de domínio
    └── InsufficientStockException.cs
```

**Por que interfaces no Core?**

```
Sem interfaces (acoplado):
┌─────────────────────────────────────────┐
│ OrderService                            │
│   └── new NpgsqlConnection(...)         │ ← Conhece PostgreSQL
│   └── new SqlCommand("SELECT...")       │ ← Conhece SQL
└─────────────────────────────────────────┘
Problema: Como testar sem banco real?

Com interfaces (desacoplado):
┌─────────────────────────────────────────┐
│ OrderService                            │
│   └── _repository.GetByIdAsync(id)      │ ← Não sabe como funciona
└─────────────────────────────────────────┘
        ↓ (injetado em runtime)
┌─────────────────────────────────────────┐
│ PostgresOrderRepository (produção)      │
│   └── SELECT * FROM orders WHERE id=... │
└─────────────────────────────────────────┘
        OU
┌─────────────────────────────────────────┐
│ FakeOrderRepository (testes)            │
│   └── return _memoryList.Find(id)       │
└─────────────────────────────────────────┘
```

#### `/src/FlashSale.Infrastructure` - Implementações Concretas

```
FlashSale.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs     ← Entity Framework DbContext
│   │
│   └── Configurations/             ← Mapeamento objeto-relacional
│       └── OrderConfiguration.cs
│           // Order.Id → coluna "id" tipo UUID
│           // Order.Items → tabela "order_items" (1:N)
│
├── Repositories/
│   └── OrderRepository.cs          ← Implementa IOrderRepository
│       public async Task<Order> GetByIdAsync(Guid id)
│       {
│           return await _context.Orders
│               .Include(o => o.Items)
│               .FirstOrDefaultAsync(o => o.Id == id);
│       }
│
├── Redis/
│   ├── RedisStreamPublisher.cs     ← Publica mensagens
│   ├── RedisStreamConsumer.cs      ← Lê mensagens
│   ├── RedisCacheService.cs        ← Cache de estoque
│   └── RedisLockService.cs         ← Distributed locks
│
└── SignalR/
    └── SignalRNotificationService.cs
        // Envia mensagem para cliente específico
        await _hubContext.Clients
            .User(userId)
            .SendAsync("OrderConfirmed", orderId);
```

---

## 4. Arquitetura do Sistema

### 4.1 O Fluxo Passo a Passo

```
PASSO 1: Clique do Usuário
┌──────────────────────────────────────────────────────────┐
│ 🖱️ Usuário clica em "COMPRAR AGORA"                      │
│                                                          │
│ Frontend (Next.js) envia:                                │
│ POST /api/v1/orders                                      │
│ {                                                        │
│   "customerId": "user-123",                              │
│   "items": [{"productId": "ingresso-vip", "qty": 2}],   │
│   "idempotencyKey": "order-abc-123-timestamp"           │
│ }                                                        │
└──────────────────────────────────────────────────────────┘
                              │
                              ▼
PASSO 2: API Recebe e Enfileira
┌──────────────────────────────────────────────────────────┐
│ 🚀 FlashSale.Api                                         │
│                                                          │
│ 1. Middleware valida rate limit (OK, < 100 req/min)     │
│ 2. Controller valida JSON (campos obrigatórios OK)      │
│ 3. Serializa para Redis Stream:                         │
│    XADD orders:pending * orderId "xxx" customerId "yyy" │
│ 4. Retorna HTTP 202 Accepted                            │
│                                                          │
│ ⏱️ Tempo total: ~5ms                                     │
└──────────────────────────────────────────────────────────┘
                              │
                              ▼
PASSO 3: Resposta Imediata ao Cliente
┌──────────────────────────────────────────────────────────┐
│ 📱 Cliente recebe:                                       │
│ HTTP 202 Accepted                                        │
│ {                                                        │
│   "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",    │
│   "status": "pending",                                   │
│   "message": "Seu pedido está sendo processado"         │
│ }                                                        │
│                                                          │
│ UI mostra: "Aguarde, processando sua compra..." 🔄      │
│ (Conecta no SignalR para receber resultado)             │
└──────────────────────────────────────────────────────────┘
                              │
                              ▼
PASSO 4: Worker Processa
┌──────────────────────────────────────────────────────────┐
│ ⚙️ FlashSale.Worker                                      │
│                                                          │
│ 1. Lê mensagem do Redis Stream                          │
│    XREADGROUP GROUP order-processors consumer-1 ...     │
│                                                          │
│ 2. Verifica idempotência (já processou esse pedido?)    │
│    GET order:processed:abc-123 → null (não processou)   │
│                                                          │
│ 3. Inicia transação PostgreSQL                          │
│    BEGIN;                                                │
│                                                          │
│ 4. Verifica estoque com LOCK                            │
│    SELECT stock FROM products WHERE id='xyz' FOR UPDATE;│
│    stock = 5 ✅                                          │
│                                                          │
│ 5. Decrementa estoque                                   │
│    UPDATE products SET stock = 3 WHERE id = 'xyz';      │
│                                                          │
│ 6. Cria pedido                                          │
│    INSERT INTO orders (...) VALUES (...);               │
│                                                          │
│ 7. Commita transação                                    │
│    COMMIT;                                               │
│                                                          │
│ 8. Marca como processado                                │
│    SET order:processed:abc-123 "1" EX 86400             │
│                                                          │
│ 9. Confirma mensagem                                    │
│    XACK orders:pending order-processors message-id      │
└──────────────────────────────────────────────────────────┘
                              │
                              ▼
PASSO 5: Notificação em Tempo Real
┌──────────────────────────────────────────────────────────┐
│ 📣 SignalR envia para o navegador:                       │
│                                                          │
│ await _hubContext.Clients                                │
│     .User("user-123")                                    │
│     .SendAsync("OrderConfirmed", new {                   │
│         orderId = "7c9e...",                            │
│         status = "confirmed",                           │
│         totalAmount = 599.98                            │
│     });                                                  │
│                                                          │
│ 📱 Cliente recebe via WebSocket:                         │
│ UI mostra: "✅ Compra realizada com sucesso!"            │
└──────────────────────────────────────────────────────────┘
```

### 4.2 Por Que HTTP 202 e Não 200?

```
HTTP 200 OK           → "Terminei de processar"
HTTP 202 Accepted     → "Recebi, vou processar depois"

No nosso caso:
- API NÃO confirma a venda
- API apenas ENFILEIRA
- Por isso usamos 202

Analogia do mundo real:
- 200 = Restaurante serve a comida
- 202 = Restaurante anota o pedido e dá uma senha
```

### 4.3 Consumer Groups Explicado

```
Problema: E se tivermos 3 Workers?

Sem Consumer Group:
┌─────────────────────────────────────────────────┐
│ Worker 1: processa mensagem A                   │
│ Worker 2: processa mensagem A (REPETIDO! 💥)    │
│ Worker 3: processa mensagem A (REPETIDO! 💥)    │
└─────────────────────────────────────────────────┘

Com Consumer Group:
┌─────────────────────────────────────────────────┐
│ Redis sabe quem já leu cada mensagem            │
│                                                 │
│ Stream: orders:pending                          │
│ Group: order-processors                         │
│                                                 │
│ mensagem A → entregue para Worker 1 ✅          │
│ mensagem B → entregue para Worker 2 ✅          │
│ mensagem C → entregue para Worker 3 ✅          │
│ mensagem D → entregue para Worker 1 ✅          │
│                                                 │
│ Cada mensagem processada por EXATAMENTE 1 worker│
└─────────────────────────────────────────────────┘
```

---

## 5. Banco de Dados

### 5.1 Entendendo Cada Tabela

#### Tabela `products`

```sql
CREATE TABLE products (
    id UUID PRIMARY KEY,        -- Identificador único
    name VARCHAR(255),          -- "Ingresso VIP Show XYZ"
    price DECIMAL(10,2),        -- 299.99
    stock INTEGER,              -- Quantidade disponível
    version INTEGER DEFAULT 1,  -- ← IMPORTANTE: Optimistic Lock
    is_flash_sale BOOLEAN,      -- Ativa recursos especiais
    sale_start_at TIMESTAMP,    -- Quando a venda começa
    sale_end_at TIMESTAMP       -- Quando a venda termina
);

-- O campo VERSION é crucial!
-- Cada UPDATE incrementa version
-- Se dois updates tentam mudar a mesma versão, um falha
```

**Exemplo de Optimistic Locking:**

```sql
-- Worker 1 lê produto
SELECT id, stock, version FROM products WHERE id = 'xyz';
-- Retorna: stock=10, version=5

-- Worker 2 também lê (ao mesmo tempo)
SELECT id, stock, version FROM products WHERE id = 'xyz';
-- Retorna: stock=10, version=5

-- Worker 1 tenta atualizar
UPDATE products 
SET stock = 9, version = 6 
WHERE id = 'xyz' AND version = 5;
-- ✅ Sucesso! 1 row affected

-- Worker 2 tenta atualizar
UPDATE products 
SET stock = 9, version = 6 
WHERE id = 'xyz' AND version = 5;
-- ❌ Falha! 0 rows affected (version já é 6)
-- Worker 2 precisa ler novamente e tentar de novo
```

#### Tabela `orders`

```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    customer_id UUID REFERENCES customers(id),
    correlation_id UUID,        -- Para rastrear nos logs
    status order_status,        -- 'pending', 'confirmed', 'failed'
    total_amount DECIMAL(10,2),
    idempotency_key VARCHAR UNIQUE,  -- ← IMPORTANTE!
    failure_reason TEXT         -- "Estoque insuficiente"
);

-- IDEMPOTENCY_KEY garante que o mesmo pedido
-- não seja processado duas vezes!
```

**Exemplo de Idempotência:**

```
Cenário: Rede instável, cliente clica 2x no botão "Comprar"

Pedido 1: idempotency_key = "order-user123-1704067200"
→ INSERT INTO orders ... → ✅ Sucesso

Pedido 2: idempotency_key = "order-user123-1704067200" (mesmo!)
→ INSERT INTO orders ... → ❌ UNIQUE violation
→ Sistema detecta e retorna o pedido existente

Resultado: Cliente não é cobrado 2x 🎉
```

#### Tabela `stock_movements`

```sql
CREATE TABLE stock_movements (
    id UUID PRIMARY KEY,
    product_id UUID,
    order_id UUID,
    movement_type movement_type,  -- 'reserve', 'confirm', 'release'
    quantity INTEGER,             -- Pode ser negativo!
    previous_stock INTEGER,       -- Estoque antes
    new_stock INTEGER            -- Estoque depois
);

-- Isso cria um LOG COMPLETO de todas as movimentações
-- Útil para auditoria e debugging
```

**Exemplo de Auditoria:**

```sql
SELECT * FROM stock_movements WHERE product_id = 'ingresso-vip';

-- id | movement_type | quantity | previous | new
--  1 | confirm       |       -2 |       10 |   8   ← Venda
--  2 | confirm       |       -1 |        8 |   7   ← Venda
--  3 | release       |       +2 |        7 |   9   ← Cancelamento
--  4 | adjust        |      +50 |        9 |  59   ← Reposição manual
```

### 5.2 Pessimistic vs Optimistic Locking

```
┌────────────────────────────────────────────────────────────┐
│                  PESSIMISTIC LOCKING                        │
├────────────────────────────────────────────────────────────┤
│ "Eu vou travar essa linha AGORA"                           │
│                                                            │
│ SELECT * FROM products WHERE id = 'xyz' FOR UPDATE;        │
│                                                            │
│ ⏳ Outros que tentarem ler ficam ESPERANDO                 │
│ ✅ Garante que ninguém mais mexe                           │
│ ❌ Mais lento (bloqueio)                                   │
│ ❌ Risco de deadlock                                       │
│                                                            │
│ Usar quando: Operações críticas (decrementar estoque)      │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                  OPTIMISTIC LOCKING                         │
├────────────────────────────────────────────────────────────┤
│ "Vou assumir que tá tudo bem e verificar no final"         │
│                                                            │
│ UPDATE products                                            │
│ SET stock = 9, version = version + 1                       │
│ WHERE id = 'xyz' AND version = 5;                          │
│                                                            │
│ Se rows_affected = 0: Alguém mudou antes, tentar de novo   │
│ ✅ Mais rápido (sem bloqueio)                              │
│ ✅ Sem risco de deadlock                                   │
│ ❌ Precisa de retry logic                                  │
│                                                            │
│ Usar quando: Operações com baixa colisão                   │
└────────────────────────────────────────────────────────────┘
```

---

## 6. APIs e Comunicação

### 6.1 REST API Detalhada

#### Criando um Pedido

```http
POST /api/v1/orders
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "customerId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "quantity": 2
    }
  ],
  "idempotencyKey": "order-f47ac10b-1704067200"
}
```

**Resposta de Sucesso (202):**
```json
{
  "orderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "pending",
  "message": "Seu pedido está sendo processado",
  "estimatedProcessingTime": "5s"
}
```

**Resposta de Erro (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "items": ["O campo items é obrigatório"],
    "items[0].quantity": ["Quantidade deve ser maior que 0"]
  }
}
```

**Resposta de Erro (429 - Rate Limit):**
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Limite de 100 requisições por minuto excedido",
  "retryAfter": 45
}
```

### 6.2 SignalR em Detalhe

```javascript
// Frontend (Next.js / React)

import * as signalR from "@microsoft/signalr";

// 1. Conectar ao Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://api.queuemaster.com/hubs/orders", {
        accessTokenFactory: () => getAuthToken()
    })
    .withAutomaticReconnect()
    .build();

// 2. Registrar handlers para eventos do servidor
connection.on("OrderConfirmed", (data) => {
    console.log("Pedido confirmado!", data);
    // { orderId: "xxx", totalAmount: 599.98 }
    showSuccessModal(`Compra realizada! Total: R$ ${data.totalAmount}`);
});

connection.on("OrderFailed", (data) => {
    console.log("Pedido falhou!", data);
    // { orderId: "xxx", reason: "Estoque insuficiente" }
    showErrorModal(data.reason);
});

connection.on("StockUpdated", (data) => {
    console.log("Estoque atualizado!", data);
    // { productId: "xxx", newStock: 47 }
    updateStockDisplay(data.productId, data.newStock);
});

// 3. Iniciar conexão
await connection.start();

// 4. Inscrever-se em updates de um pedido específico
await connection.invoke("SubscribeToOrder", orderId);

// 5. Inscrever-se em updates de estoque de um produto
await connection.invoke("SubscribeToProduct", productId);
```

```csharp
// Backend (SignalR Hub)

public class OrderNotificationHub : Hub
{
    // Quando cliente chama "SubscribeToOrder"
    public async Task SubscribeToOrder(string orderId)
    {
        // Adiciona conexão a um "grupo" do SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order:{orderId}");
    }
    
    // Quando cliente chama "SubscribeToProduct"
    public async Task SubscribeToProduct(string productId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"product:{productId}");
    }
}

// No Worker, quando pedido é processado:
public class NotificationService
{
    private readonly IHubContext<OrderNotificationHub> _hubContext;
    
    public async Task NotifyOrderConfirmed(Order order)
    {
        // Envia para todos que estão inscrito nesse pedido
        await _hubContext.Clients
            .Group($"order:{order.Id}")
            .SendAsync("OrderConfirmed", new 
            {
                orderId = order.Id,
                totalAmount = order.TotalAmount
            });
    }
}
```

---

## 7. Engenharia UTM

### 7.1 O Que é UTM?

```
UTM = Urchin Tracking Module (Google Analytics criou)

Serve para rastrear DE ONDE veio o visitante.

Exemplo de URL com UTM:
https://queuemaster.com/ingressos?
    utm_source=facebook           ← Veio do Facebook
    &utm_medium=cpc               ← Anúncio pago (Cost Per Click)
    &utm_campaign=black_friday    ← Campanha Black Friday
    &utm_content=banner_topo      ← Clicou no banner do topo
    &utm_term=show_rock           ← Buscou por "show rock"
```

### 7.2 Fluxo de Captura

```
ETAPA 1: Usuário clica em anúncio do Facebook
┌──────────────────────────────────────────────┐
│ Facebook Ad → queuemaster.com?utm_source=fb  │
└──────────────────────────────────────────────┘
                    │
                    ▼
ETAPA 2: Landing Page captura UTM
┌──────────────────────────────────────────────┐
│ // hooks/useUTM.ts                           │
│                                              │
│ const utm = {                                │
│   source: url.searchParams.get('utm_source'),│
│   medium: url.searchParams.get('utm_medium'),│
│   campaign: url.searchParams.get('utm_campaign')│
│ };                                           │
│                                              │
│ sessionStorage.setItem('utm', JSON.stringify(utm));│
└──────────────────────────────────────────────┘
                    │
                    ▼
ETAPA 3: Checkout envia UTM junto com pedido
┌──────────────────────────────────────────────┐
│ POST /api/v1/orders                          │
│ {                                            │
│   "customerId": "...",                       │
│   "items": [...],                            │
│   "utm": {                                   │
│     "source": "facebook",                    │
│     "medium": "cpc",                         │
│     "campaign": "black_friday"               │
│   }                                          │
│ }                                            │
└──────────────────────────────────────────────┘
                    │
                    ▼
ETAPA 4: Banco armazena para análise
┌──────────────────────────────────────────────┐
│ INSERT INTO utm_tracking (                   │
│   order_id, utm_source, utm_campaign,        │
│   landing_page, device_type                  │
│ ) VALUES (...);                              │
└──────────────────────────────────────────────┘
```

### 7.3 Relatórios de ROI

```sql
-- Qual campanha gerou mais receita?
SELECT 
    utm_campaign,
    COUNT(*) as total_vendas,
    SUM(o.total_amount) as receita_total,
    AVG(o.total_amount) as ticket_medio
FROM utm_tracking ut
JOIN orders o ON ut.order_id = o.id
WHERE o.status = 'confirmed'
GROUP BY utm_campaign
ORDER BY receita_total DESC;

-- Resultado:
-- campaign       | vendas | receita   | ticket
-- black_friday   | 1500   | 450000.00 | 300.00
-- natal          | 800    | 200000.00 | 250.00
-- lancamento     | 300    | 150000.00 | 500.00
```

---

## 8. Escalabilidade

### 8.1 Escala Horizontal vs Vertical

```
ESCALA VERTICAL (Scale Up)
┌─────────────────────────────────────────────┐
│ "Comprar servidor mais potente"             │
│                                             │
│ Antes: 4 CPU, 16GB RAM                      │
│ Depois: 32 CPU, 128GB RAM                   │
│                                             │
│ ✅ Simples de implementar                   │
│ ❌ Limite físico (não existe CPU infinita)  │
│ ❌ Caro (servidores grandes são caros)      │
│ ❌ Single point of failure                  │
└─────────────────────────────────────────────┘

ESCALA HORIZONTAL (Scale Out) ← O que usamos!
┌─────────────────────────────────────────────┐
│ "Adicionar mais servidores"                 │
│                                             │
│ Antes: 1 servidor                           │
│ Depois: 10 servidores (menores)             │
│                                             │
│ ✅ Sem limite teórico                       │
│ ✅ Mais barato (servidores pequenos)        │
│ ✅ Alta disponibilidade                     │
│ ❌ Mais complexo (load balancer, state)     │
└─────────────────────────────────────────────┘
```

### 8.2 Auto-Scaling com Kubernetes

```yaml
# Horizontal Pod Autoscaler (HPA)

# O Kubernetes monitora métricas e adiciona/remove pods automaticamente

apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: flashsale-api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: flashsale-api
  
  # Limites
  minReplicas: 3    # Mínimo 3 pods sempre (alta disponibilidade)
  maxReplicas: 50   # Máximo 50 pods (custo controlado)
  
  # Quando escalar?
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70  # Se CPU > 70%, adiciona pods
```

**Exemplo Visual de Auto-Scaling:**

```
Normal (dia comum):
┌─────┐ ┌─────┐ ┌─────┐
│ API │ │ API │ │ API │
│  1  │ │  2  │ │  3  │
└─────┘ └─────┘ └─────┘
CPU: 30%  25%    28%

Flash Sale começa (10:00):
┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐
│ API │ │ API │ │ API │ │ API │ │ API │ │ API │ + mais...
│  1  │ │  2  │ │  3  │ │  4  │ │  5  │ │  6  │
└─────┘ └─────┘ └─────┘ └─────┘ └─────┘ └─────┘
CPU: 68%  72%    65%    70%    67%    71%

Flash Sale termina (10:30):
┌─────┐ ┌─────┐ ┌─────┐
│ API │ │ API │ │ API │
│  1  │ │  2  │ │  3  │
└─────┘ └─────┘ └─────┘
CPU: 25%  20%    22%
(pods extras são removidos automaticamente)
```

### 8.3 Cache em Camadas

```
┌────────────────────────────────────────────────────────────┐
│ CAMADA 1: Browser Cache (mais rápido, menos controle)      │
├────────────────────────────────────────────────────────────┤
│ Cache-Control: public, max-age=300                         │
│                                                            │
│ O navegador guarda a resposta por 5 minutos               │
│ Nem precisa fazer requisição ao servidor                  │
│                                                            │
│ Bom para: imagens, CSS, JS, dados que mudam pouco         │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│ CAMADA 2: CDN Cache (Cloudflare, Fastly)                   │
├────────────────────────────────────────────────────────────┤
│ Servidores espalhados pelo mundo                          │
│                                                            │
│ Usuário em São Paulo → CDN São Paulo (10ms)               │
│ Usuário em Tokyo → CDN Tokyo (10ms)                       │
│ (Sem CDN: ambos iriam ao servidor em Virginia = 200ms)    │
│                                                            │
│ Bom para: conteúdo estático, páginas de produto           │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│ CAMADA 3: Redis Cache (servidor)                           │
├────────────────────────────────────────────────────────────┤
│ Cache em memória no backend                               │
│                                                            │
│ GET stock:product-xyz → "47"                              │
│                                                            │
│ Se tiver no Redis: retorna instantaneamente               │
│ Se não tiver: consulta PostgreSQL, salva no Redis         │
│                                                            │
│ Bom para: estoque, sessões, dados frequentemente lidos    │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│ CAMADA 4: PostgreSQL (fonte da verdade)                    │
├────────────────────────────────────────────────────────────┤
│ Banco de dados principal                                  │
│                                                            │
│ Usado quando:                                             │
│ - Cache não existe ou expirou                             │
│ - Escrita (INSERT, UPDATE)                                │
│ - Transações ACID são necessárias                         │
└────────────────────────────────────────────────────────────┘
```

---

## 9. Padrões de Design

### 9.1 Circuit Breaker

```
Problema: Serviço externo (pagamento) está fora do ar

Sem Circuit Breaker:
┌─────────────────────────────────────────────┐
│ Requisição 1 → Timeout 30s → Falha         │
│ Requisição 2 → Timeout 30s → Falha         │
│ Requisição 3 → Timeout 30s → Falha         │
│ ...                                         │
│ 💥 Threads esgotadas, sistema trava        │
└─────────────────────────────────────────────┘

Com Circuit Breaker:
┌─────────────────────────────────────────────┐
│ Requisição 1 → Timeout 30s → Falha ❌       │
│ Requisição 2 → Timeout 30s → Falha ❌       │
│ Requisição 3 → Timeout 30s → Falha ❌       │
│                                             │
│ [CIRCUIT ABERTO! 50% de falha]             │
│                                             │
│ Requisição 4 → Falha imediata (0ms) 🔌     │
│ Requisição 5 → Falha imediata (0ms) 🔌     │
│                                             │
│ (Após 30s, tenta novamente)                │
│ Requisição 100 → Sucesso! ✅                │
│                                             │
│ [CIRCUIT FECHADO]                          │
└─────────────────────────────────────────────┘
```

**Implementação com Polly (.NET):**

```csharp
services.AddHttpClient<IPaymentService, PaymentService>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 5,   // 5 falhas
            durationOfBreak: TimeSpan.FromSeconds(30)  // Abre por 30s
        ));
```

### 9.2 Retry com Exponential Backoff

```
Problema: Falha temporária de rede

Retry Simples (ruim):
┌─────────────────────────────────────────────┐
│ Tentativa 1 → Falha → Retry imediato       │
│ Tentativa 2 → Falha → Retry imediato       │
│ Tentativa 3 → Falha → Retry imediato       │
│                                             │
│ 💥 Sobrecarrega o serviço que está mal     │
└─────────────────────────────────────────────┘

Retry com Backoff Exponencial (bom):
┌─────────────────────────────────────────────┐
│ Tentativa 1 → Falha → Espera 1s            │
│ Tentativa 2 → Falha → Espera 2s            │
│ Tentativa 3 → Falha → Espera 4s            │
│ Tentativa 4 → Falha → Espera 8s            │
│ Tentativa 5 → Sucesso! ✅                   │
│                                             │
│ Dá tempo do serviço se recuperar           │
└─────────────────────────────────────────────┘
```

### 9.3 Dead Letter Queue (DLQ)

```
Problema: Mensagem falha repetidamente

┌──────────────────────────────────────────────────────────┐
│ Fila Principal: orders:pending                           │
├──────────────────────────────────────────────────────────┤
│ Mensagem {orderId: "abc", ...}                           │
│                                                          │
│ Tentativa 1: Erro de banco → Requeue                    │
│ Tentativa 2: Erro de rede → Requeue                     │
│ Tentativa 3: Erro desconhecido → Move para DLQ         │
└──────────────────────────────────────────────────────────┘
                    │
                    ▼
┌──────────────────────────────────────────────────────────┐
│ Dead Letter Queue: orders:dlq                            │
├──────────────────────────────────────────────────────────┤
│ {                                                        │
│   "originalMessage": {orderId: "abc", ...},             │
│   "error": "Connection refused",                        │
│   "attempts": 3,                                         │
│   "failedAt": "2024-01-15T10:30:00Z"                    │
│ }                                                        │
│                                                          │
│ ✅ Mensagem não é perdida                               │
│ ✅ Pode ser analisada depois                            │
│ ✅ Pode ser reprocessada manualmente                    │
└──────────────────────────────────────────────────────────┘
```

---

## 10. DevOps e Infraestrutura

### 10.1 Docker Compose Explicado

```yaml
version: '3.8'

services:
  # A API que recebe requisições HTTP
  api:
    build: ./src/FlashSale.Api     # Constrói a partir do Dockerfile
    ports:
      - "5000:8080"                 # Host:Container
    environment:
      - ConnectionStrings__PostgreSQL=Host=postgres;...
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres                    # Espera postgres subir primeiro
      - redis

  # O Worker que processa a fila
  worker:
    build: ./src/FlashSale.Worker
    deploy:
      replicas: 3                   # 3 instâncias do worker
    depends_on:
      - postgres
      - redis

  # Banco de dados
  postgres:
    image: postgres:16-alpine       # Imagem oficial
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: flashsale
    volumes:
      - postgres_data:/var/lib/postgresql/data  # Persiste dados

  # Cache e Fila
  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes      # Persistência

volumes:
  postgres_data:    # Volume nomeado para persistir dados do postgres
```

### 10.2 Comandos Úteis

```bash
# Subir todo o ambiente
docker-compose up -d

# Ver logs da API
docker-compose logs -f api

# Ver logs dos workers
docker-compose logs -f worker

# Escalar workers para 10 instâncias
docker-compose up -d --scale worker=10

# Acessar PostgreSQL
docker-compose exec postgres psql -U postgres -d flashsale

# Acessar Redis CLI
docker-compose exec redis redis-cli

# Derrubar tudo
docker-compose down

# Derrubar e remover volumes (CUIDADO: perde dados!)
docker-compose down -v
```

### 10.3 Pipeline CI/CD

```
┌─────────────────────────────────────────────────────────┐
│ TRIGGER: git push main                                  │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│ ETAPA 1: Build & Test                                   │
├─────────────────────────────────────────────────────────┤
│ 1. Checkout do código                                   │
│ 2. dotnet restore (baixa dependências)                  │
│ 3. dotnet build (compila)                               │
│ 4. dotnet test --filter Category=Unit                   │
│                                                         │
│ Se falhar: ❌ PR não pode ser mergeado                  │
└─────────────────────────────────────────────────────────┘
                    │ ✅
                    ▼
┌─────────────────────────────────────────────────────────┐
│ ETAPA 2: Integration Tests                              │
├─────────────────────────────────────────────────────────┤
│ 1. docker-compose up (sobe Redis + Postgres)            │
│ 2. dotnet test --filter Category=Integration            │
│ 3. docker-compose down                                  │
└─────────────────────────────────────────────────────────┘
                    │ ✅
                    ▼
┌─────────────────────────────────────────────────────────┐
│ ETAPA 3: Load Test                                      │
├─────────────────────────────────────────────────────────┤
│ 1. Deploy em ambiente de staging                        │
│ 2. k6 run load-test.js                                  │
│ 3. Verifica: p95 < 500ms? Error rate < 1%?             │
│                                                         │
│ Se falhar: ⚠️ Alerta no Slack, não bloqueia deploy     │
└─────────────────────────────────────────────────────────┘
                    │ ✅
                    ▼
┌─────────────────────────────────────────────────────────┐
│ ETAPA 4: Deploy                                         │
├─────────────────────────────────────────────────────────┤
│ 1. docker build -t api:v1.2.3                           │
│ 2. docker push registry.com/api:v1.2.3                  │
│ 3. kubectl apply -f k8s/                                │
│ 4. kubectl rollout status deployment/flashsale-api     │
│                                                         │
│ Rolling update: pods antigos → pods novos gradualmente  │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Conclusão

### Checklist do que você aprendeu:

- [ ] **Problema**: Race condition e sobrecarga em flash sales
- [ ] **Solução**: Buffer assíncrono com Redis Streams
- [ ] **HTTP 202**: Indica processamento assíncrono
- [ ] **Consumer Groups**: Distribuição de trabalho entre workers
- [ ] **Optimistic Locking**: Campo `version` para evitar conflitos
- [ ] **Idempotência**: `idempotency_key` evita duplicação
- [ ] **SignalR**: Notificação real-time via WebSocket
- [ ] **Circuit Breaker**: Proteção contra cascata de falhas
- [ ] **DLQ**: Mensagens que falharam não são perdidas
- [ ] **Auto-scaling**: Kubernetes HPA adiciona pods sob demanda

---

**Próximo passo:** Implementar o código base do projeto!

🚀 Boa sorte com o QueueMaster!
