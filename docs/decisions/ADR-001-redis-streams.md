# ADR-001: Uso de Redis Streams para Mensageria

## Status
**Aceito** - 2026-01-09

## Contexto

O sistema QueueMaster precisa processar milhares de pedidos por segundo durante 
eventos de flash sale. A solução de mensageria deve garantir:

- Latência ultra-baixa (< 10ms)
- Durabilidade de mensagens
- Processamento distribuído (múltiplos workers)
- Simplicidade operacional

## Decisão

Usaremos **Redis Streams** como sistema de mensageria em vez de RabbitMQ, 
Kafka ou AWS SQS.

## Justificativa

### Por que Redis Streams?

1. **Latência**: < 1ms para operações XADD/XREAD
2. **Simplicidade**: Já usamos Redis para cache, reduz complexidade
3. **Consumer Groups**: Suporte nativo para processamento distribuído
4. **Persistência**: AOF garante durabilidade
5. **Acknowledgment**: XACK garantia mensagens processadas

### Comparação

| Critério | Redis Streams | RabbitMQ | Kafka |
|----------|---------------|----------|-------|
| Latência | < 1ms | 5-10ms | 10-50ms |
| Throughput | 100k+ msg/s | 50k msg/s | 1M+ msg/s |
| Complexidade | Baixa | Média | Alta |
| Durabilidade | AOF | Disco | Disco |
| Escalabilidade | Boa | Boa | Excelente |

## Consequências

### Positivas
- Infraestrutura simplificada (menos um serviço)
- Latência mínima para processamento
- Curva de aprendizado menor
- Custo operacional reduzido

### Negativas
- Menos features que Kafka (compaction, exactly-once)
- Armazenamento limitado pela RAM disponível
- Menos ecosistema de ferramentas

### Mitigações
- Implementar TTL nas mensagens processadas
- Monitorar uso de memória do Redis
- Configurar maxmemory-policy apropriada

## Alternativas Consideradas

### RabbitMQ
- ❌ Latência maior
- ❌ Mais um serviço para operar
- ✅ Features de roteamento avançado (não necessário)

### Apache Kafka
- ❌ Complexidade operacional alta
- ❌ Overkill para nosso volume
- ✅ Melhor para event sourcing long-term

### AWS SQS
- ❌ Vendor lock-in
- ❌ Latência de rede
- ✅ Managed service (menos ops)

## Referências
- [Redis Streams Documentation](https://redis.io/docs/data-types/streams/)
- [Redis Streams vs Kafka](https://redis.com/blog/redis-streams-vs-kafka/)
