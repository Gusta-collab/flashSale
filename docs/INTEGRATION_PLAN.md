# 🔗 Planejamento de Integração - QueueMaster

## Visão Geral

Este documento define como os componentes do sistema serão integrados e testados juntos.

---

## 1. Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ORDEM DE IMPLEMENTAÇÃO                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Semana 1                                                                   │
│  ┌──────────────┐                                                           │
│  │ FlashSale    │ ← Primeiro! Zero dependências                            │
│  │   .Core      │                                                           │
│  └──────┬───────┘                                                           │
│         │                                                                   │
│  Semana 2                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐     ┌──────────────┐                                     │
│  │ FlashSale    │     │ Docker       │                                     │
│  │   .Infra     │     │ Compose      │ ← Paralelo                          │
│  └──────┬───────┘     └──────┬───────┘                                     │
│         │                    │                                              │
│  Semana 3                    │                                              │
│         ▼                    ▼                                              │
│  ┌──────────────┐     ┌──────────────┐                                     │
│  │ FlashSale    │◀────│ Redis        │                                     │
│  │   .Api       │     │ Integration  │                                     │
│  └──────┬───────┘     └──────────────┘                                     │
│         │                                                                   │
│  Semana 4                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐                                                           │
│  │ FlashSale    │                                                           │
│  │   .Worker    │                                                           │
│  └──────┬───────┘                                                           │
│         │                                                                   │
│  Semana 5                                                                   │
│         ▼                                                                   │
│  ┌──────────────┐                                                           │
│  │ SignalR +    │                                                           │
│  │ Integration  │                                                           │
│  └──────────────┘                                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Pontos de Integração

### 2.1 API ↔ Redis Streams

| Componente | Responsabilidade |
|------------|------------------|
| OrdersController | Recebe request, valida, publica |
| RedisStreamPublisher | Serializa e envia para stream |
| Redis Stream | Armazena mensagens pendentes |

**Teste de Integração:**
```csharp
[Fact]
public async Task CreateOrder_ShouldPublishToRedisStream()
{
    // Arrange
    var request = new CreateOrderRequest { ... };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/orders", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    
    // Verificar que mensagem está no Redis
    var messages = await _redis.StreamReadAsync("orders:pending");
    messages.Should().ContainSingle();
}
```

---

### 2.2 Redis Streams ↔ Worker

| Componente | Responsabilidade |
|------------|------------------|
| Redis Stream | Armazena mensagens |
| OrderConsumer | Lê com XREADGROUP |
| OrderProcessingHandler | Processa pedido |

**Teste de Integração:**
```csharp
[Fact]
public async Task Worker_ShouldProcessOrderFromStream()
{
    // Arrange
    await _redis.StreamAddAsync("orders:pending", message);
    
    // Act
    await Task.Delay(5000); // Aguardar processamento
    
    // Assert
    var order = await _dbContext.Orders.FindAsync(orderId);
    order.Status.Should().Be(OrderStatus.Confirmed);
}
```

---

### 2.3 Worker ↔ PostgreSQL

| Componente | Responsabilidade |
|------------|------------------|
| OrderProcessingHandler | Orquestra processamento |
| ProductRepository | Locking e decremento |
| OrderRepository | Persistência do pedido |

**Pontos Críticos:**
- SELECT FOR UPDATE para lock de estoque
- Transação única para decremento + insert
- Optimistic locking com version

---

### 2.4 Worker ↔ SignalR

| Componente | Responsabilidade |
|------------|------------------|
| NotificationService | Envia eventos |
| IHubContext | Acesso ao Hub |
| Client Browser | Recebe WebSocket |

**Teste de Integração:**
```csharp
[Fact]
public async Task Worker_ShouldNotifyClientViaSignalR()
{
    // Arrange
    var hubConnection = new HubConnectionBuilder()
        .WithUrl("http://localhost/hubs/orders")
        .Build();
    
    var notificationReceived = false;
    hubConnection.On<OrderConfirmedEvent>("OrderConfirmed", (e) => {
        notificationReceived = true;
    });
    
    await hubConnection.StartAsync();
    await hubConnection.InvokeAsync("SubscribeToOrder", orderId);
    
    // Act - Processar pedido
    await _redis.StreamAddAsync("orders:pending", orderMessage);
    await Task.Delay(5000);
    
    // Assert
    notificationReceived.Should().BeTrue();
}
```

---

## 3. Ambiente de Integração

### docker-compose.integration.yml

```yaml
version: '3.8'

services:
  api:
    build: ./src/FlashSale.Api
    ports: ["5000:8080"]
    depends_on: [postgres, redis]
    environment:
      - ASPNETCORE_ENVIRONMENT=Integration

  worker:
    build: ./src/FlashSale.Worker
    depends_on: [postgres, redis]

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_PASSWORD: integration

  redis:
    image: redis:7-alpine
```

---

## 4. Checklist de Integração

### Fase 1: Core → Infrastructure
```markdown
- [ ] Entidades mapeiam corretamente para tabelas
- [ ] Repositories implementam interfaces do Core
- [ ] Migrations funcionam sem erros
```

### Fase 2: Infrastructure → API
```markdown
- [ ] Controllers recebem repositories via DI
- [ ] Validação retorna erros apropriados
- [ ] Health check verifica banco
```

### Fase 3: API → Redis
```markdown
- [ ] POST /orders publica no stream
- [ ] Mensagem contém todos os campos
- [ ] Idempotency key é preservada
```

### Fase 4: Redis → Worker
```markdown
- [ ] Consumer Group está configurado
- [ ] Worker processa mensagens
- [ ] ACK é enviado após sucesso
- [ ] DLQ recebe falhas
```

### Fase 5: Worker → SignalR
```markdown
- [ ] Notificação é enviada após processamento
- [ ] Cliente recebe via WebSocket
- [ ] Múltiplas instâncias funcionam (Redis backplane)
```

---

## 5. Smoke Tests

### Após cada integração, executar:

```bash
# 1. Subir ambiente
docker-compose -f docker-compose.integration.yml up -d

# 2. Aguardar health
until curl -s http://localhost:5000/health | grep -q "Healthy"; do sleep 1; done

# 3. Criar pedido
curl -X POST http://localhost:5000/api/v1/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"test","items":[{"productId":"xxx","quantity":1}]}'

# 4. Verificar status
curl http://localhost:5000/api/v1/orders/{orderId}/status

# 5. Verificar estoque
psql -U postgres -d flashsale -c "SELECT stock FROM products WHERE id='xxx'"
```

---

## 6. Critérios de Aceite da Integração

| Critério | Métrica | Target |
|----------|---------|--------|
| Latência E2E | P95 | < 5s |
| Mensagens perdidas | Count | 0 |
| Race conditions | Estoque negativo | 0 |
| Notificações | Entregues | 100% |

---

📅 **Criado:** 2026-01-09  
📝 **Status:** Planejado
