# 📊 01 - Objetivos e Progresso do Projeto

## QueueMaster: Sistema de Vendas de Alta Demanda

---

## 1. Visão Geral

### 1.1 Objetivo do Projeto

Desenvolver um sistema de vendas de alta demanda (Flash Sale) capaz de:
- Processar **5.000+ requisições/segundo**
- Garantir **consistência de dados** (zero overselling)
- Manter **latência < 500ms** em 95% das requisições
- Operar com **alta disponibilidade** (99.9% uptime)

### 1.2 Problema que Resolve

```
Cenário: Black Friday / Venda de Ingressos
┌─────────────────────────────────────────────────────────────┐
│ • 500.000 usuários tentando comprar                         │
│ • 10.000 itens disponíveis                                  │
│ • Janela de 10 minutos                                      │
│                                                             │
│ PROBLEMA: Sistema tradicional                               │
│ ├── Banco de dados sobrecarregado                          │
│ ├── Race conditions (estoque negativo)                     │
│ └── Timeout e perda de vendas                              │
│                                                             │
│ SOLUÇÃO: QueueMaster                                        │
│ ├── Buffer assíncrono (Redis Streams)                      │
│ ├── Processamento sequencial (Workers)                     │
│ └── Notificação real-time (SignalR)                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Marcos do Projeto (Milestones)

### 2.1 Roadmap Visual

```
Semana 1    Semana 2    Semana 3    Semana 4    Semana 5
   │           │           │           │           │
   ▼           ▼           ▼           ▼           ▼
┌──────┐   ┌──────┐   ┌──────┐   ┌──────┐   ┌──────┐
│  M1  │───│  M2  │───│  M3  │───│  M4  │───│  M5  │
│Setup │   │ API  │   │Worker│   │Redis │   │Signal│
└──────┘   └──────┘   └──────┘   └──────┘   └──────┘

Semana 6    Semana 7    Semana 8    Semana 9
   │           │           │           │
   ▼           ▼           ▼           ▼
┌──────┐   ┌──────┐   ┌──────┐   ┌──────┐
│  M6  │───│  M7  │───│  M8  │───│  M9  │
│Tests │   │ Load │   │Docker│   │ Docs │
└──────┘   └──────┘   └──────┘   └──────┘
```

### 2.2 Detalhamento dos Marcos

| Marco | Nome | Descrição | Entregáveis | Status |
|-------|------|-----------|-------------|--------|
| **M1** | Setup | Estrutura Clean Architecture | Solution, projetos, DI | 🔲 Pendente |
| **M2** | API | Gateway de entrada | Controllers, DTOs, Middleware | 🔲 Pendente |
| **M3** | Worker | Processador de fila | Consumer, Handlers | 🔲 Pendente |
| **M4** | Redis | Integração messaging | Streams, Cache, Locks | 🔲 Pendente |
| **M5** | SignalR | Real-time | Hub, Notifications | 🔲 Pendente |
| **M6** | Tests | Testes Unit/Integration | 80% coverage | 🔲 Pendente |
| **M7** | Load | Testes de carga | k6 scripts, relatórios | 🔲 Pendente |
| **M8** | Docker | Containerização | Dockerfile, Compose, K8s | 🔲 Pendente |
| **M9** | Docs | Documentação final | README, API docs | 🔲 Pendente |

---

## 3. Métricas de Sucesso (KPIs)

### 3.1 Performance

| Métrica | Target | Atual | Status |
|---------|--------|-------|--------|
| Throughput | ≥ 5,000 req/s | - | 🔲 |
| Latência P50 | < 100ms | - | 🔲 |
| Latência P95 | < 500ms | - | 🔲 |
| Latência P99 | < 1000ms | - | 🔲 |

### 3.2 Confiabilidade

| Métrica | Target | Atual | Status |
|---------|--------|-------|--------|
| Taxa de Erro | < 0.1% | - | 🔲 |
| Disponibilidade | 99.9% | - | 🔲 |
| Recovery Time | < 30s | - | 🔲 |

### 3.3 Qualidade de Código

| Métrica | Target | Atual | Status |
|---------|--------|-------|--------|
| Cobertura de Testes | > 80% | - | 🔲 |
| Vulnerabilidades Críticas | 0 | - | 🔲 |
| Code Smells | < 10 | - | 🔲 |
| Duplicação | < 3% | - | 🔲 |

### 3.4 Negócio

| Métrica | Target | Atual | Status |
|---------|--------|-------|--------|
| Overselling | 0 casos | - | 🔲 |
| Pedidos perdidos | 0 | - | 🔲 |
| Tempo de processamento | < 5s | - | 🔲 |

---

## 4. Critérios de Aceite por Marco

### M1 - Setup do Projeto

```markdown
✅ Critérios de Aceite:
[ ] Solution .NET 8 criado com estrutura Clean Architecture
[ ] Projetos: Api, Worker, Core, Application, Infrastructure, Tests
[ ] Dependency Injection configurado
[ ] Logging estruturado (Serilog)
[ ] Health checks implementados
[ ] Docker Compose funcional (postgres + redis)
[ ] README com instruções de setup
```

### M2 - API Gateway

```markdown
✅ Critérios de Aceite:
[ ] POST /api/v1/orders retorna 202 Accepted
[ ] GET /api/v1/orders/{id}/status funcional
[ ] GET /api/v1/products funcional
[ ] Rate limiting implementado (100 req/min)
[ ] Correlation ID em todas as requests
[ ] Swagger/OpenAPI documentado
[ ] Validação de entrada (FluentValidation)
```

### M3 - Worker Service

```markdown
✅ Critérios de Aceite:
[ ] Consumer lendo Redis Stream
[ ] Consumer Group configurado
[ ] Processamento com retry (3 tentativas)
[ ] Dead Letter Queue implementada
[ ] Graceful shutdown
[ ] Métricas expostas (Prometheus)
```

### M4 - Redis Integration

```markdown
✅ Critérios de Aceite:
[ ] Redis Streams para mensageria
[ ] Cache de estoque (30s TTL)
[ ] Distributed Lock para operações críticas
[ ] Idempotency check via Redis
[ ] Connection pooling configurado
```

### M5 - SignalR

```markdown
✅ Critérios de Aceite:
[ ] Hub /hubs/orders funcional
[ ] Evento OrderConfirmed enviado
[ ] Evento OrderFailed enviado
[ ] Evento StockUpdated enviado
[ ] Suporte a múltiplas instâncias (Redis backplane)
```

### M6 - Testes

```markdown
✅ Critérios de Aceite:
[ ] Cobertura > 80%
[ ] Testes unitários para Services
[ ] Testes unitários para Validators
[ ] Testes unitários para Entities
[ ] Testes de integração para API
[ ] Testes de integração para Redis
[ ] Testes de integração para PostgreSQL
```

### M7 - Load Testing

```markdown
✅ Critérios de Aceite:
[ ] Script k6 criado
[ ] Cenário: 5000 VUs simulados
[ ] Relatório de P95 < 500ms
[ ] Relatório de error rate < 0.1%
[ ] Teste de estresse (até quebrar)
[ ] Documentação de limites do sistema
```

### M8 - Docker & DevOps

```markdown
✅ Critérios de Aceite:
[ ] Dockerfile multi-stage para API
[ ] Dockerfile multi-stage para Worker
[ ] docker-compose.yml funcional
[ ] GitHub Actions CI/CD
[ ] Kubernetes manifests (opcional)
[ ] Scan de segurança (Trivy)
```

### M9 - Documentação

```markdown
✅ Critérios de Aceite:
[ ] README completo
[ ] Arquitetura documentada
[ ] API documentada (Swagger)
[ ] Runbook de operação
[ ] Troubleshooting guide
[ ] Post para LinkedIn preparado
```

---

## 5. Registro de Progresso

### 5.1 Log de Atividades

| Data | Marco | Atividade | Responsável | Status |
|------|-------|-----------|-------------|--------|
| 2026-01-09 | M0 | Documentação inicial criada | Team | ✅ |
| - | M1 | - | - | 🔲 |

### 5.2 Bloqueios Atuais

| ID | Descrição | Impacto | Ação Necessária | Owner |
|----|-----------|---------|-----------------|-------|
| - | Nenhum bloqueio atual | - | - | - |

### 5.3 Decisões Técnicas

| Data | Decisão | Justificativa | Alternativa Descartada |
|------|---------|---------------|------------------------|
| 2026-01-09 | Redis Streams | Melhor que RabbitMQ para latência | RabbitMQ, Kafka |
| 2026-01-09 | PostgreSQL | ACID forte, extensões | MongoDB, MySQL |
| 2026-01-09 | .NET 8 | Performance, tipagem | Node.js, Go |

---

## 6. Próximos Passos

### 6.1 Imediato (Esta Semana)

1. [ ] Criar estrutura de pastas do projeto
2. [ ] Configurar Solution .NET 8
3. [ ] Implementar entidades do Core
4. [ ] Configurar Docker Compose inicial

### 6.2 Curto Prazo (Próximas 2 Semanas)

1. [ ] Implementar API básica
2. [ ] Implementar Worker básico
3. [ ] Integrar com Redis

### 6.3 Médio Prazo (Próximo Mês)

1. [ ] Completar testes
2. [ ] Executar load tests
3. [ ] Documentar e publicar

---

📅 **Próxima Revisão:** Após conclusão do M1  
📝 **Responsável:** Tech Lead
