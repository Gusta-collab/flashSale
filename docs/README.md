# 📚 Documentação do Projeto QueueMaster

## Estrutura de Documentação

```
/docs
├── CHANGELOG.md              ← Histórico de mudanças
├── architecture.md           ← Arquitetura do sistema
├── README.md                 ← Este arquivo (índice)
└── /decisions                ← Architecture Decision Records
    ├── ADR-001-redis-streams.md
    ├── ADR-002-postgresql.md
    ├── ADR-003-dotnet8.md
    └── template.md
```

---

## Documentos Principais

| Documento | Descrição |
|-----------|-----------|
| [CHANGELOG](./CHANGELOG.md) | Histórico de todas as mudanças |
| [Arquitetura](./architecture.md) | Visão geral do sistema |

---

## Decisões de Arquitetura (ADRs)

| ADR | Título | Status |
|-----|--------|--------|
| [001](./decisions/ADR-001-redis-streams.md) | Redis Streams para Mensageria | Aceito |
| [002](./decisions/ADR-002-postgresql.md) | PostgreSQL como Banco Principal | Aceito |
| [003](./decisions/ADR-003-dotnet8.md) | .NET 8 como Stack Backend | Aceito |

---

## Regras de Desenvolvimento

Consulte a pasta `/rules` para os padrões do projeto:
- [Índice de Regras](../rules/00-INDEX.md)

---

📅 **Última atualização**: 2026-01-09
