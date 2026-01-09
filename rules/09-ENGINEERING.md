# ⚙️ 09 - Regras de Engenharia

## Padrões de Arquitetura e Código

---

## 1. Regras de Código

| ID | Regra | Severidade | Limite |
|----|-------|------------|--------|
| ENG-01 | XML docs em membros públicos | 🔴 Error | Obrigatório |
| ENG-02 | Métodos pequenos | 🟠 Warning | Max 30 linhas |
| ENG-03 | Classes focadas | 🟠 Warning | Max 300 linhas |
| ENG-04 | Complexidade ciclomática | 🟠 Warning | Max 10 |
| ENG-05 | Usar async/await para I/O | 🔴 Error | Obrigatório |
| ENG-06 | Não bloquear threads | 🔴 Error | Proibido .Result |
| ENG-07 | CancellationToken em async | 🟠 Warning | Recomendado |
| ENG-08 | Injeção via construtor | 🔴 Error | Obrigatório |
| ENG-09 | Aninhamento máximo | 🟠 Warning | Max 3 níveis |
| ENG-10 | Parâmetros máximos | 🟠 Warning | Max 4 params |

---

## 2. Dependências de Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    REGRAS DE DEPENDÊNCIA                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  FlashSale.Api                                              │
│    └── PODE referenciar → Application, Core, Infrastructure│
│                                                             │
│  FlashSale.Worker                                           │
│    └── PODE referenciar → Application, Core, Infrastructure│
│                                                             │
│  FlashSale.Application                                      │
│    └── PODE referenciar → Core                              │
│    └── NÃO PODE referenciar → Infrastructure ❌             │
│                                                             │
│  FlashSale.Core                                             │
│    └── NÃO PODE referenciar → NINGUÉM (zero deps) ❌        │
│                                                             │
│  FlashSale.Infrastructure                                   │
│    └── PODE referenciar → Core                              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Definition of Done (DoD)

```markdown
Para considerar uma tarefa "DONE":

☐ Código implementado seguindo Clean Code
☐ Testes unitários escritos e passando
☐ Testes de integração (se aplicável)
☐ Documentação XML em membros públicos
☐ Code review aprovado (mín 1 reviewer)
☐ Sem vulnerabilidades no scan
☐ Pipeline CI/CD passando
☐ Documentação atualizada (se necessário)
☐ Migrations criadas (se mudança no banco)
```

---

## 4. Padrões Obrigatórios

### 4.1 Dependency Injection

```csharp
// ✅ CORRETO: Injeção via construtor
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}

// ❌ INCORRETO: Instanciar diretamente
public class OrderService
{
    private readonly OrderRepository _repository = new OrderRepository(); // NÃO!
}
```

### 4.2 Async/Await

```csharp
// ✅ CORRETO: Async all the way
public async Task<Order> GetOrderAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ INCORRETO: Bloqueando thread
public Order GetOrder(Guid id)
{
    return _repository.GetByIdAsync(id).Result; // NUNCA!
}
```

### 4.3 Validação de Null

```csharp
// ✅ CORRETO: Guard clauses no início
public async Task ProcessAsync(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    ArgumentNullException.ThrowIfNull(order.Items);
    
    // Lógica principal...
}
```

---

## 5. Estrutura de Projeto

```
/src
├── FlashSale.Api           ← Presentation Layer
│   ├── Controllers/
│   ├── Middleware/
│   └── DTOs/
│
├── FlashSale.Application   ← Application Layer
│   ├── Services/
│   ├── Commands/
│   └── Queries/
│
├── FlashSale.Core          ← Domain Layer (ZERO deps!)
│   ├── Entities/
│   ├── Interfaces/
│   └── Exceptions/
│
├── FlashSale.Infrastructure← Infrastructure Layer
│   ├── Data/
│   ├── Repositories/
│   └── Redis/
│
└── FlashSale.Tests
    ├── Unit/
    └── Integration/
```

---

## 6. Checklist de Engenharia

```markdown
Antes de cada PR:

- [ ] Código segue padrões SOLID?
- [ ] Dependências respeitadas (Core sem deps)?
- [ ] Métodos < 30 linhas?
- [ ] Classes < 300 linhas?
- [ ] Async/await usado corretamente?
- [ ] Nulls tratados?
- [ ] Logs informativos?
```

---

📅 **Referência:** Clean Architecture - Robert C. Martin
