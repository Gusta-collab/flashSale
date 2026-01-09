# 🧪 06 - Testes Automatizados

## Guia de Testes do Projeto

---

## 1. Estrutura de Testes

```
/src/FlashSale.Tests
├── /Unit              ← Testes unitários (sem deps externas)
│   ├── /Services
│   ├── /Validators
│   └── /Entities
├── /Integration       ← Testes de integração (com deps)
│   ├── /Api
│   ├── /Redis
│   └── /Database
├── /Load              ← Testes de carga (k6)
│   └── k6-load-test.js
└── /E2E               ← Testes end-to-end
```

---

## 2. Cobertura Mínima

| Camada | Cobertura | Prioridade |
|--------|-----------|------------|
| Core (Domínio) | 90% | 🔴 Crítica |
| Application | 85% | 🔴 Crítica |
| Infrastructure | 70% | 🟠 Alta |
| API | 75% | 🟠 Alta |

---

## 3. Padrão AAA (Arrange-Act-Assert)

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task ProcessOrder_WithSufficientStock_ShouldConfirm()
{
    // ═══════════════════════════════════════
    // ARRANGE: Preparar o cenário
    // ═══════════════════════════════════════
    var product = new Product { Id = Guid.NewGuid(), Stock = 10 };
    var request = new CreateOrderRequest 
    { 
        Items = new[] { new OrderItem { ProductId = product.Id, Quantity = 2 } }
    };
    
    _mockRepository
        .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);

    // ═══════════════════════════════════════
    // ACT: Executar a ação
    // ═══════════════════════════════════════
    var result = await _service.ProcessOrderAsync(request);

    // ═══════════════════════════════════════
    // ASSERT: Verificar o resultado
    // ═══════════════════════════════════════
    Assert.NotNull(result);
    Assert.Equal(OrderStatus.Confirmed, result.Status);
    _mockRepository.Verify(r => r.UpdateAsync(
        It.Is<Product>(p => p.Stock == 8), 
        It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## 4. Nomenclatura de Testes

```csharp
// Formato: [Método]_[Cenário]_[ResultadoEsperado]

[Fact]
public async Task ProcessOrder_WithInsufficientStock_ShouldThrowException()

[Fact]
public async Task GetById_WithInvalidId_ShouldReturnNull()

[Fact]
public async Task CreateOrder_WithDuplicateKey_ShouldReturnExisting()
```

---

## 5. Testes de Integração

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task OrdersController_CreateOrder_ShouldReturn202()
{
    // Arrange
    await using var application = new WebApplicationFactory<Program>();
    using var client = application.CreateClient();
    
    var request = new CreateOrderRequest { /* ... */ };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/orders", request);

    // Assert
    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    
    var result = await response.Content.ReadFromJsonAsync<OrderResponse>();
    Assert.NotNull(result?.OrderId);
}
```

---

## 6. Teste de Carga (k6)

```javascript
// k6-load-test.js
import http from 'k6/http';
import { check } from 'k6';

export const options = {
  scenarios: {
    flash_sale: {
      executor: 'ramping-vus',
      stages: [
        { duration: '10s', target: 100 },
        { duration: '30s', target: 5000 },
        { duration: '10s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],  // P95 < 500ms
    errors: ['rate<0.01'],              // Error < 1%
  },
};

export default function () {
  const payload = JSON.stringify({
    customerId: `customer-${__VU}`,
    items: [{ productId: 'xxx', quantity: 1 }],
  });

  const res = http.post(`${__ENV.BASE_URL}/api/v1/orders`, payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(res, {
    'status is 202': (r) => r.status === 202,
  });
}
```

---

## 7. Comandos

```bash
# Rodar todos os testes
dotnet test

# Apenas unitários
dotnet test --filter Category=Unit

# Apenas integração
dotnet test --filter Category=Integration

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# k6 load test
k6 run --env BASE_URL=http://localhost:5000 k6-load-test.js
```

---

## 8. Checklist de Testes

```markdown
- [ ] Testes unitários para lógica de negócio?
- [ ] Testes de edge cases (null, vazio, limites)?
- [ ] Testes de erro (exceções esperadas)?
- [ ] Mocks configurados corretamente?
- [ ] Testes de integração para endpoints?
- [ ] Cobertura mínima atingida?
```

---

📅 **Framework:** xUnit + Moq + FluentAssertions
