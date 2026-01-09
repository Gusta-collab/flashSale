# 📝 08 - Padrões de Documentação

## Guia de Documentação de Código

---

## 1. Regra Principal

> **Todo código público DEVE ter documentação XML.**

---

## 2. XML Documentation (C#)

### 2.1 Classes

```csharp
/// <summary>
/// Serviço responsável pelo processamento de pedidos em flash sales.
/// Implementa padrões de idempotência e retry.
/// </summary>
/// <remarks>
/// Utiliza Redis Streams como buffer e PostgreSQL para persistência.
/// </remarks>
public class OrderService : IOrderService
{
    // ...
}
```

### 2.2 Métodos

```csharp
/// <summary>
/// Processa um pedido de forma assíncrona com garantia de idempotência.
/// </summary>
/// <param name="request">Dados do pedido a processar.</param>
/// <param name="cancellationToken">Token para cancelamento.</param>
/// <returns>Resultado do processamento com status e detalhes.</returns>
/// <exception cref="InsufficientStockException">Estoque insuficiente.</exception>
/// <exception cref="DuplicateOrderException">Pedido duplicado.</exception>
public async Task<OrderResult> ProcessOrderAsync(
    CreateOrderRequest request,
    CancellationToken cancellationToken = default)
{
    // ...
}
```

### 2.3 Propriedades

```csharp
/// <summary>
/// Identificador único do pedido.
/// </summary>
public Guid Id { get; set; }

/// <summary>
/// Status atual do pedido (pending, confirmed, failed).
/// </summary>
public OrderStatus Status { get; set; }

/// <summary>
/// Valor total do pedido em reais.
/// </summary>
public decimal TotalAmount { get; set; }
```

---

## 3. Comentários Inline

### 3.1 Quando Usar

```csharp
public async Task<bool> DecrementStockAsync(Guid productId, int quantity)
{
    // ═══════════════════════════════════════════════════════
    // PASSO 1: Verificar cache primeiro
    // Evita hit no banco para operações frequentes
    // ═══════════════════════════════════════════════════════
    var cachedStock = await _cache.GetAsync<int?>($"stock:{productId}");
    
    if (cachedStock.HasValue && cachedStock.Value < quantity)
        return false;  // Early return: sem estoque suficiente

    // ═══════════════════════════════════════════════════════
    // PASSO 2: Lock otimista com retry
    // Tenta até 3 vezes em caso de conflito de versão
    // ═══════════════════════════════════════════════════════
    const int maxRetries = 3;
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        // Buscar produto com versão atual
        var product = await _context.Products.FindAsync(productId);
        
        // Tentar atualizar...
    }
    
    return false;
}
```

### 3.2 O Que Comentar

| Comentar ✅ | Não Comentar ❌ |
|------------|----------------|
| **Por quê** foi feito assim | **O que** o código faz (óbvio) |
| Decisões não óbvias | Código auto-explicativo |
| TODOs com ticket | Código comentado (deletar) |
| Workarounds temporários | Comentários desatualizados |

---

## 4. README do Projeto

```markdown
# 🚀 QueueMaster

Sistema de vendas de alta demanda.

## Pré-requisitos
- .NET 8 SDK
- Docker

## Instalação
\`\`\`bash
git clone https://github.com/org/queuemaster.git
docker-compose up -d
\`\`\`

## Uso
\`\`\`bash
dotnet run --project src/FlashSale.Api
\`\`\`

## Testes
\`\`\`bash
dotnet test
\`\`\`

## Documentação
- [Arquitetura](./docs/architecture.md)
- [API](http://localhost:5000/swagger)
```

---

## 5. Changelog

```markdown
# Changelog

## [1.1.0] - 2026-01-15
### Added
- SignalR notifications

### Fixed
- Race condition in stock decrement

## [1.0.0] - 2026-01-01
### Added
- Initial release
```

---

## 6. Checklist

```markdown
- [ ] Classes públicas têm /// summary?
- [ ] Métodos públicos têm /// summary + params?
- [ ] Exceções estão documentadas?
- [ ] README está atualizado?
- [ ] Comentários explicam o "porquê"?
```

---

📅 **Referência:** Microsoft XML Documentation
