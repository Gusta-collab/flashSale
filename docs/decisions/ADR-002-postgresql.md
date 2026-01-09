# ADR-002: PostgreSQL como Banco de Dados Principal

## Status
**Aceito** - 2026-01-09

## Contexto

O sistema QueueMaster requer um banco de dados para armazenar:
- Pedidos e itens
- Produtos e estoque
- Clientes
- Movimentações de estoque (auditoria)

Requisitos críticos:
- Transações ACID para garantir consistência
- Suporte a locking (pessimistic/optimistic)
- Performance para alta concorrência
- Escalabilidade futura

## Decisão

Usaremos **PostgreSQL 16** como banco de dados principal.

## Justificativa

### Por que PostgreSQL?

1. **ACID Completo**: Transações garantem consistência em operações críticas
2. **Locking Robusto**: `SELECT FOR UPDATE` para controle de concorrência
3. **Performance**: Excelente para workloads transacionais
4. **Extensível**: suporte a JSON, full-text search, extensões
5. **Open Source**: Sem custos de licenciamento
6. **Maturidade**: 35+ anos de desenvolvimento

### Comparação

| Critério | PostgreSQL | MySQL | MongoDB | SQL Server |
|----------|------------|-------|---------|------------|
| ACID | ✅ Completo | ✅ | ⚠️ Parcial | ✅ |
| Locking | ✅ Row-level | ✅ | ⚠️ Document | ✅ |
| JSON Support | ✅ JSONB | ✅ | ✅ Nativo | ✅ |
| Custo | Gratuito | Gratuito | Gratuito | Pago |
| Extensões | ✅ Muitas | ⚠️ Poucas | ❌ | ⚠️ |

## Consequências

### Positivas
- Garantia de consistência em operações de estoque
- Ecossistema maduro de ferramentas
- Excelente documentação
- Comunidade ativa

### Negativas
- Escalabilidade horizontal mais complexa que NoSQL
- Requer tuning para alta performance

### Mitigações
- Read replicas para distribuir leitura
- Connection pooling (PgBouncer)
- Particionamento de tabelas se necessário

## Configurações Recomendadas

```sql
-- Otimizações para alta concorrência
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '768MB';
ALTER SYSTEM SET work_mem = '4MB';
```

## Referências
- [PostgreSQL Performance](https://wiki.postgresql.org/wiki/Performance_Optimization)
- [PostgreSQL vs MySQL](https://www.postgresql.org/about/advantages/)
