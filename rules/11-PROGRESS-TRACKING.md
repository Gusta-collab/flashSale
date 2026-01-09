# 📋 11 - Regras de Documentação de Progresso

## Registro Obrigatório de Progresso do Projeto

---

## 1. Regra Principal

> **TODO progresso DEVE ser documentado imediatamente após sua conclusão.**

---

## 2. O Que Documentar

| Evento | Onde Registrar | Obrigatório |
|--------|----------------|-------------|
| Conclusão de Marco (Milestone) | `CHANGELOG.md` + `01-PROGRESS.md` | ✅ Sim |
| Feature implementada | `CHANGELOG.md` | ✅ Sim |
| Bug corrigido | `CHANGELOG.md` | ✅ Sim |
| Decisão técnica | `docs/decisions/ADR-XXX.md` | ✅ Sim |
| Mudança de arquitetura | `docs/architecture.md` | ✅ Sim |
| Início de nova tarefa | `01-PROGRESS.md` (status 🔄) | ✅ Sim |
| Bloqueio identificado | `01-PROGRESS.md` (bloqueios) | ✅ Sim |

---

## 3. Formato do CHANGELOG

```markdown
# Changelog

Todas as mudanças notáveis do projeto serão documentadas aqui.
Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

## [Unreleased]

### Added
- Descrição da feature adicionada (#PR)

### Changed
- Descrição da mudança (#PR)

### Fixed
- Descrição do bug corrigido (#PR)

### Security
- Descrição da correção de segurança (#PR)

---

## [1.0.0] - 2026-01-15

### Added
- Implementação inicial do sistema
- API de criação de pedidos
- Worker de processamento

### Infrastructure
- Docker Compose configurado
- CI/CD com GitHub Actions
```

---

## 4. Atualização de Status em 01-PROGRESS.md

### 4.1 Status de Marcos

| Status | Símbolo | Quando Usar |
|--------|---------|-------------|
| Pendente | 🔲 | Não iniciado |
| Em progresso | 🔄 | Iniciado, em andamento |
| Concluído | ✅ | Finalizado e testado |
| Bloqueado | ⛔ | Impedido por dependência |

### 4.2 Exemplo de Atualização

```markdown
## 2. Marcos do Projeto

| Marco | Nome | Status | Data Conclusão |
|-------|------|--------|----------------|
| M1 | Setup | ✅ Concluído | 2026-01-09 |
| M2 | API | 🔄 Em progresso | - |
| M3 | Worker | 🔲 Pendente | - |
```

---

## 5. Architecture Decision Records (ADR)

### 5.1 Quando Criar ADR

- Nova tecnologia adotada
- Padrão de design escolhido
- Mudança significativa de arquitetura
- Trade-off importante decidido

### 5.2 Template ADR

```markdown
# ADR-001: Uso de Redis Streams para Mensageria

## Status
Aceito

## Contexto
Precisamos de uma solução de mensageria com baixa latência para 
processar milhares de pedidos por segundo.

## Decisão
Usaremos Redis Streams ao invés de RabbitMQ ou Kafka.

## Justificativa
- Latência ultra-baixa (< 1ms)
- Já usamos Redis para cache
- Consumer Groups nativos
- Menor complexidade operacional

## Consequências
### Positivas
- Simplifica a infraestrutura
- Reduz latência

### Negativas
- Menos features que Kafka
- Armazenamento limitado pela RAM

## Alternativas Consideradas
1. RabbitMQ - Descartado por latência
2. Kafka - Descartado por complexidade
3. AWS SQS - Descartado por vendor lock-in
```

---

## 6. Checklist de Documentação de Progresso

### Ao Iniciar uma Tarefa
```markdown
- [ ] Atualizar status para 🔄 em 01-PROGRESS.md
- [ ] Registrar data de início
- [ ] Identificar dependências
```

### Ao Concluir uma Tarefa
```markdown
- [ ] Atualizar status para ✅ em 01-PROGRESS.md
- [ ] Registrar data de conclusão
- [ ] Adicionar entrada no CHANGELOG.md
- [ ] Criar ADR se houve decisão técnica importante
- [ ] Atualizar README se necessário
```

### Ao Encontrar Bloqueio
```markdown
- [ ] Atualizar status para ⛔ em 01-PROGRESS.md
- [ ] Documentar bloqueio na seção de Bloqueios
- [ ] Identificar ação necessária e responsável
```

---

## 7. Estrutura de Documentação

```
/docs
├── CHANGELOG.md           ← Histórico de mudanças
├── architecture.md        ← Visão geral da arquitetura
├── api.md                 ← Documentação da API
├── runbook.md             ← Guia de operações
├── /decisions             ← ADRs
│   ├── ADR-001-redis-streams.md
│   ├── ADR-002-postgresql.md
│   └── template.md
└── /diagrams              ← Diagramas técnicos
    ├── system-architecture.png
    └── database-erd.png
```

---

## 8. Frequência de Atualização

| Documento | Frequência | Responsável |
|-----------|------------|-------------|
| `01-PROGRESS.md` | A cada mudança de status | Dev responsável |
| `CHANGELOG.md` | A cada PR mergeado | Autor do PR |
| ADRs | A cada decisão técnica | Tech Lead |
| `README.md` | A cada mudança significativa | Time |

---

## 9. Automação (Opcional)

### GitHub Actions para Verificar CHANGELOG

```yaml
name: Check CHANGELOG

on:
  pull_request:
    branches: [main, develop]

jobs:
  check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Check CHANGELOG updated
        run: |
          if ! git diff --name-only origin/develop | grep -q "CHANGELOG.md"; then
            echo "⚠️ CHANGELOG.md não foi atualizado neste PR"
            echo "Por favor, adicione uma entrada descrevendo suas mudanças"
            exit 1
          fi
```

---

📅 **Revisão:** Após cada Sprint/Marco
📝 **Referência:** Keep a Changelog + ADR
