# 🔒 03 - Desenvolvimento Seguro (DevSecOps)

## Guia de Segurança para o Projeto QueueMaster

---

## 1. Regras Obrigatórias

| ID | Regra | Severidade |
|----|-------|------------|
| **SEC-001** | NUNCA commitar secrets | 🔴 Crítica |
| **SEC-002** | Validar TODA entrada do usuário | 🔴 Crítica |
| **SEC-003** | Usar prepared statements | 🔴 Crítica |
| **SEC-004** | HTTPS em todas as comunicações | 🔴 Crítica |
| **SEC-005** | Implementar autenticação JWT | 🔴 Crítica |
| **SEC-006** | Rate limiting obrigatório | 🟠 Alta |
| **SEC-007** | Logar operações sensíveis | 🟠 Alta |

---

## 2. Gerenciamento de Secrets

### 2.1 Onde Armazenar

```yaml
# ✅ CORRETO: Variáveis de ambiente
services:
  api:
    environment:
      - ConnectionStrings__PostgreSQL=${POSTGRES_CONNECTION}
      - Jwt__Secret=${JWT_SECRET}
```

### 2.2 .gitignore Obrigatório

```gitignore
# Secrets - NUNCA COMMITAR
.env
.env.local
appsettings.Development.json
secrets/
*.pem
*.key
```

### 2.3 Se um Secret Vazar

1. ⏱️ **Imediato**: Revogar/rotacionar o secret
2. 🔄 **1 hora**: Gerar e deployar novos secrets
3. 📝 **24 horas**: Verificar logs e documentar
4. 🛡️ **1 semana**: Adicionar pre-commit hooks

---

## 3. Validação de Entrada

```csharp
// ✅ CORRETO: FluentValidation
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.Items.Count).LessThanOrEqualTo(10);
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(10);
        });
    }
}
```

---

## 4. Proteção contra Injection

```csharp
// ✅ CORRETO: Entity Framework (parameterizado)
var order = await _context.Orders
    .Where(o => o.Id == orderId)
    .FirstOrDefaultAsync();

// ✅ CORRETO: Dapper com parâmetros
const string sql = "SELECT * FROM orders WHERE id = @OrderId";
var order = await _connection.QueryAsync<Order>(sql, new { OrderId = orderId });

// ❌ NUNCA: Concatenação de SQL
var sql = $"SELECT * FROM orders WHERE id = '{orderId}'"; // SQL INJECTION!
```

---

## 5. Autenticação JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
```

---

## 6. Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PerIpLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

---

## 7. OWASP Top 10 Checklist

- [ ] **A01** - Broken Access Control (verificar ownership)
- [ ] **A02** - Cryptographic Failures (HTTPS, criptografia)
- [ ] **A03** - Injection (prepared statements)
- [ ] **A04** - Insecure Design (rate limiting)
- [ ] **A05** - Security Misconfiguration (headers)
- [ ] **A06** - Vulnerable Components (dependabot)
- [ ] **A07** - Auth Failures (brute force protection)
- [ ] **A08** - Software Integrity (CI/CD seguro)
- [ ] **A09** - Security Logging (logar eventos)
- [ ] **A10** - SSRF (validar URLs)

---

## 8. Logging de Segurança

```csharp
// ✅ SEMPRE logar
_logger.LogInformation("Login bem-sucedido. UserId: {UserId}", userId);
_logger.LogWarning("Acesso negado. UserId: {UserId}", userId);

// ❌ NUNCA logar
_logger.LogInformation("Password: {Password}", password);  // NUNCA!
_logger.LogInformation("Token: {Token}", jwtToken);       // NUNCA!
```

---

## 9. Checklist de PR

```markdown
- [ ] Nenhum secret hardcoded?
- [ ] Inputs validados?
- [ ] Autenticação/autorização verificada?
- [ ] Nenhum SQL concatenado?
- [ ] Rate limiting aplicado?
- [ ] Logs seguros (sem dados sensíveis)?
```

---

📅 **Referência:** OWASP Top 10 2021
