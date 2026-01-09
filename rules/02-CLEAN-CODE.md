# 🧹 02 - Padrões Clean Code

## Guia de Código Limpo e Manutenível

---

## 1. Princípios Fundamentais

### 1.1 Os 4 Pilares do Clean Code

```
┌─────────────────────────────────────────────────────────────┐
│                    CLEAN CODE                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  📖 LEGIBILIDADE     │  🔧 MANUTENIBILIDADE                │
│  Código lido como    │  Fácil de modificar                 │
│  uma história        │  sem quebrar                        │
│                      │                                      │
│  🧪 TESTABILIDADE    │  ♻️ REUSABILIDADE                   │
│  Fácil de testar     │  Componentes                        │
│  isoladamente        │  reutilizáveis                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Regra de Ouro

> "Deixe o código mais limpo do que você encontrou."
> — Boy Scout Rule

---

## 2. Nomenclatura

### 2.1 Regras Gerais

| Elemento | Estilo | Exemplo ✅ | Contra-exemplo ❌ |
|----------|--------|-----------|-------------------|
| Classes | PascalCase | `OrderService` | `orderService` |
| Interfaces | I + PascalCase | `IOrderRepository` | `OrderRepositoryInterface` |
| Métodos | PascalCase | `ProcessOrderAsync` | `process_order` |
| Variáveis | camelCase | `orderId` | `OrderId`, `order_id` |
| Constantes | PascalCase | `MaxRetryAttempts` | `MAX_RETRY_ATTEMPTS` |
| Privados | _camelCase | `_logger` | `logger`, `m_logger` |
| Parâmetros | camelCase | `customerId` | `CustomerId` |

### 2.2 Nomes Descritivos

```csharp
// ✅ CORRETO: Nomes que revelam intenção

// Variáveis
int daysSinceLastOrder;
bool isCustomerEligibleForDiscount;
List<Order> pendingOrders;

// Métodos
public async Task<bool> HasSufficientStockAsync(Guid productId, int quantity);
public Order CalculateTotalWithDiscounts(Order order, Customer customer);
public void SendOrderConfirmationEmail(Order order);

// Classes
public class OrderProcessingService { }
public class CustomerEligibilityValidator { }
public class StockReservationHandler { }
```

```csharp
// ❌ INCORRETO: Nomes genéricos ou abreviados

// Variáveis
int d;           // O que é "d"?
bool flag;       // Qual flag?
List<Order> lst; // Abreviação desnecessária

// Métodos
public void Process();        // Processar o quê?
public bool Check();          // Checar o quê?
public void DoStuff();        // ???

// Classes
public class Manager { }      // Manager de quê?
public class Helper { }       // Helper para quê?
public class Utils { }        // Anti-pattern
```

### 2.3 Convenções de Nomenclatura Async

```csharp
// ✅ Métodos async SEMPRE terminam com "Async"
public async Task<Order> GetOrderAsync(Guid id);
public async Task ProcessOrderAsync(Order order);
public async Task<bool> ValidateStockAsync(Guid productId);

// ❌ INCORRETO
public async Task<Order> GetOrder(Guid id);  // Falta "Async"
```

---

## 3. Estrutura de Classes

### 3.1 Ordem dos Membros

```csharp
public class OrderService : IOrderService
{
    // ═══════════════════════════════════════
    // 1. CONSTANTES (primeiro)
    // ═══════════════════════════════════════
    private const int MaxRetryAttempts = 3;
    private const string CacheKeyPrefix = "order:";

    // ═══════════════════════════════════════
    // 2. CAMPOS PRIVADOS READONLY
    // ═══════════════════════════════════════
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly ICacheService _cache;

    // ═══════════════════════════════════════
    // 3. CONSTRUTOR
    // ═══════════════════════════════════════
    public OrderService(
        IOrderRepository orderRepository,
        ILogger<OrderService> logger,
        ICacheService cache)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    // ═══════════════════════════════════════
    // 4. PROPRIEDADES PÚBLICAS
    // ═══════════════════════════════════════
    public int ProcessedOrdersCount { get; private set; }

    // ═══════════════════════════════════════
    // 5. MÉTODOS PÚBLICOS
    // ═══════════════════════════════════════
    public async Task<OrderResult> ProcessOrderAsync(CreateOrderRequest request)
    {
        // Implementação...
    }

    // ═══════════════════════════════════════
    // 6. MÉTODOS PRIVADOS (por último)
    // ═══════════════════════════════════════
    private async Task<bool> ValidateRequestAsync(CreateOrderRequest request)
    {
        // Implementação...
    }
}
```

### 3.2 Tamanho Máximo

| Elemento | Limite | Ação se Exceder |
|----------|--------|-----------------|
| Classe | 300 linhas | Extrair para classes menores |
| Método | 30 linhas | Extrair para métodos auxiliares |
| Parâmetros | 4 parâmetros | Criar objeto de request |
| Aninhamento | 3 níveis | Extrair early returns |

---

## 4. Estrutura de Métodos

### 4.1 Princípio da Responsabilidade Única

```csharp
// ✅ CORRETO: Cada método faz UMA coisa

public class OrderService
{
    public async Task<OrderResult> ProcessOrderAsync(CreateOrderRequest request)
    {
        // Orquestra, mas delega a responsabilidade
        await ValidateRequestAsync(request);
        var order = await CreateOrderAsync(request);
        await ReserveStockAsync(order);
        await SaveOrderAsync(order);
        await NotifyCustomerAsync(order);
        
        return new OrderResult(order);
    }

    private async Task ValidateRequestAsync(CreateOrderRequest request)
    {
        // Apenas valida
    }

    private async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        // Apenas cria o objeto Order
    }

    private async Task ReserveStockAsync(Order order)
    {
        // Apenas reserva estoque
    }

    private async Task SaveOrderAsync(Order order)
    {
        // Apenas persiste
    }

    private async Task NotifyCustomerAsync(Order order)
    {
        // Apenas notifica
    }
}
```

```csharp
// ❌ INCORRETO: Método fazendo tudo

public async Task<OrderResult> ProcessOrderAsync(CreateOrderRequest request)
{
    // Validação inline
    if (request == null) throw new ArgumentNullException();
    if (request.Items == null || !request.Items.Any()) throw new ValidationException();
    foreach (var item in request.Items)
    {
        if (item.Quantity <= 0) throw new ValidationException();
        // ... 20 linhas de validação
    }

    // Criação do pedido inline
    var order = new Order
    {
        Id = Guid.NewGuid(),
        // ... 15 linhas de mapeamento
    };

    // Verificação de estoque inline
    foreach (var item in order.Items)
    {
        var product = await _context.Products.FindAsync(item.ProductId);
        if (product.Stock < item.Quantity)
        {
            // ... 10 linhas de tratamento
        }
        // ... mais 20 linhas
    }

    // E assim continua por 200+ linhas...
}
```

### 4.2 Early Return Pattern

```csharp
// ✅ CORRETO: Early returns reduzem aninhamento

public async Task<Order?> GetOrderAsync(Guid orderId)
{
    // Guard clauses primeiro
    if (orderId == Guid.Empty)
    {
        _logger.LogWarning("OrderId inválido: {OrderId}", orderId);
        return null;
    }

    var order = await _repository.GetByIdAsync(orderId);
    
    if (order == null)
    {
        _logger.LogInformation("Pedido não encontrado: {OrderId}", orderId);
        return null;
    }

    if (order.Status == OrderStatus.Cancelled)
    {
        _logger.LogInformation("Pedido cancelado: {OrderId}", orderId);
        return null;
    }

    // Fluxo principal com 0 níveis de aninhamento
    return order;
}
```

```csharp
// ❌ INCORRETO: Aninhamento excessivo (Arrow anti-pattern)

public async Task<Order?> GetOrderAsync(Guid orderId)
{
    if (orderId != Guid.Empty)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order != null)
        {
            if (order.Status != OrderStatus.Cancelled)
            {
                return order;  // Código principal escondido
            }
            else
            {
                _logger.LogInformation("Cancelado");
                return null;
            }
        }
        else
        {
            _logger.LogInformation("Não encontrado");
            return null;
        }
    }
    else
    {
        _logger.LogWarning("Inválido");
        return null;
    }
}
```

---

## 5. Princípios SOLID

### 5.1 Single Responsibility Principle (SRP)

```csharp
// ✅ CORRETO: Uma classe, uma responsabilidade

public class OrderValidator
{
    public ValidationResult Validate(CreateOrderRequest request)
    {
        // Apenas valida pedidos
    }
}

public class OrderRepository
{
    public async Task SaveAsync(Order order)
    {
        // Apenas persiste pedidos
    }
}

public class OrderNotifier
{
    public async Task NotifyAsync(Order order)
    {
        // Apenas notifica sobre pedidos
    }
}
```

```csharp
// ❌ INCORRETO: Classe "faz-tudo"

public class OrderManager  // "Manager" é um code smell
{
    public ValidationResult Validate(CreateOrderRequest request) { }
    public async Task SaveAsync(Order order) { }
    public async Task NotifyAsync(Order order) { }
    public async Task ProcessPaymentAsync(Order order) { }
    public async Task UpdateInventoryAsync(Order order) { }
    public async Task GenerateInvoiceAsync(Order order) { }
    // 50 métodos depois...
}
```

### 5.2 Open/Closed Principle (OCP)

```csharp
// ✅ CORRETO: Aberto para extensão, fechado para modificação

// Interface define o contrato
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessAsync(Payment payment);
    bool CanHandle(PaymentMethod method);
}

// Implementações específicas
public class CreditCardProcessor : IPaymentProcessor
{
    public bool CanHandle(PaymentMethod method) => method == PaymentMethod.CreditCard;
    public async Task<PaymentResult> ProcessAsync(Payment payment) { /* ... */ }
}

public class PixProcessor : IPaymentProcessor
{
    public bool CanHandle(PaymentMethod method) => method == PaymentMethod.Pix;
    public async Task<PaymentResult> ProcessAsync(Payment payment) { /* ... */ }
}

// Adicionar novo método de pagamento NÃO altera código existente
public class BoletoProcessor : IPaymentProcessor
{
    public bool CanHandle(PaymentMethod method) => method == PaymentMethod.Boleto;
    public async Task<PaymentResult> ProcessAsync(Payment payment) { /* ... */ }
}

// Service usa a abstração
public class PaymentService
{
    private readonly IEnumerable<IPaymentProcessor> _processors;

    public async Task<PaymentResult> ProcessAsync(Payment payment)
    {
        var processor = _processors.FirstOrDefault(p => p.CanHandle(payment.Method))
            ?? throw new UnsupportedPaymentMethodException(payment.Method);
        
        return await processor.ProcessAsync(payment);
    }
}
```

### 5.3 Liskov Substitution Principle (LSP)

```csharp
// ✅ CORRETO: Subtipos podem substituir tipos base

public abstract class Notification
{
    public abstract Task SendAsync(string recipient, string message);
}

public class EmailNotification : Notification
{
    public override async Task SendAsync(string recipient, string message)
    {
        // Envia email
        await _emailClient.SendAsync(recipient, message);
    }
}

public class SmsNotification : Notification
{
    public override async Task SendAsync(string recipient, string message)
    {
        // Envia SMS
        await _smsClient.SendAsync(recipient, message);
    }
}

// Funciona com qualquer Notification
public class NotificationService
{
    public async Task NotifyAllAsync(List<Notification> notifications, string message)
    {
        foreach (var notification in notifications)
        {
            await notification.SendAsync(notification.Recipient, message);
        }
    }
}
```

### 5.4 Interface Segregation Principle (ISP)

```csharp
// ✅ CORRETO: Interfaces pequenas e coesas

public interface IOrderReader
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
}

public interface IOrderWriter
{
    Task CreateAsync(Order order);
    Task UpdateAsync(Order order);
}

public interface IOrderDeleter
{
    Task DeleteAsync(Guid id);
}

// Implementação pode escolher o que implementar
public class OrderReadOnlyRepository : IOrderReader
{
    // Apenas leitura
}

public class OrderRepository : IOrderReader, IOrderWriter
{
    // Leitura e escrita
}
```

```csharp
// ❌ INCORRETO: Interface "gordona"

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
    Task CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
    Task<byte[]> GenerateReportAsync();          // Não pertence aqui
    Task SendNotificationAsync(Order order);     // Não pertence aqui
    Task<decimal> CalculateTaxAsync(Order order); // Não pertence aqui
}
```

### 5.5 Dependency Inversion Principle (DIP)

```csharp
// ✅ CORRETO: Depende de abstrações

public class OrderService
{
    // Depende de interfaces, não implementações
    private readonly IOrderRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<OrderService> _logger;

    // Dependências injetadas via construtor
    public OrderService(
        IOrderRepository repository,
        IMessagePublisher publisher,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }
}

// Configuração no DI Container
services.AddScoped<IOrderRepository, PostgresOrderRepository>();
services.AddScoped<IMessagePublisher, RedisStreamPublisher>();
```

```csharp
// ❌ INCORRETO: Acoplamento com implementações

public class OrderService
{
    // Depende de implementações concretas
    private readonly PostgresOrderRepository _repository;
    private readonly RedisStreamPublisher _publisher;

    public OrderService()
    {
        // Instancia diretamente - impossível de testar
        _repository = new PostgresOrderRepository("connection-string");
        _publisher = new RedisStreamPublisher("redis:6379");
    }
}
```

---

## 6. Tratamento de Erros

### 6.1 Exceções Específicas

```csharp
// ✅ CORRETO: Criar exceções de domínio

public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Estoque insuficiente para produto {productId}. " +
               $"Solicitado: {requested}, Disponível: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableStock = available;
    }
}

public class OrderNotFoundException : DomainException
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"Pedido {orderId} não encontrado")
    {
        OrderId = orderId;
    }
}
```

### 6.2 Tratamento Correto

```csharp
// ✅ CORRETO: Capturar exceções específicas e logar

public async Task<Order> ProcessOrderAsync(CreateOrderRequest request)
{
    try
    {
        return await ProcessInternalAsync(request);
    }
    catch (InsufficientStockException ex)
    {
        _logger.LogWarning(ex, 
            "Estoque insuficiente para produto {ProductId}. " +
            "Solicitado: {Requested}, Disponível: {Available}",
            ex.ProductId, ex.RequestedQuantity, ex.AvailableStock);
        throw; // Re-throw para o controller tratar
    }
    catch (DuplicateOrderException ex)
    {
        _logger.LogInformation(ex,
            "Pedido duplicado detectado: {IdempotencyKey}",
            ex.IdempotencyKey);
        return await GetExistingOrderAsync(ex.OrderId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, 
            "Erro inesperado ao processar pedido para customer {CustomerId}",
            request.CustomerId);
        throw new OrderProcessingException("Erro ao processar pedido", ex);
    }
}
```

```csharp
// ❌ INCORRETO: Swallow exception ou catch genérico

public async Task<Order?> ProcessOrderBad(CreateOrderRequest request)
{
    try
    {
        return await ProcessInternalAsync(request);
    }
    catch (Exception)
    {
        return null;  // Esconde o erro completamente!
    }
}
```

---

## 7. Checklist de Clean Code

### Antes de Commitar

```markdown
[ ] Nomes são descritivos e revelam intenção?
[ ] Métodos têm menos de 30 linhas?
[ ] Classes têm menos de 300 linhas?
[ ] Não há código duplicado?
[ ] Não há números mágicos (usar constantes)?
[ ] Não há código comentado?
[ ] Complexidade ciclomática < 10?
[ ] Máximo 3 níveis de aninhamento?
[ ] Todos os métodos públicos têm XML docs?
[ ] Exceções são tratadas corretamente?
[ ] Logs são informativos?
[ ] SOLID está sendo respeitado?
```

---

📅 **Próxima Revisão:** Mensal  
📝 **Referência:** Clean Code - Robert C. Martin
