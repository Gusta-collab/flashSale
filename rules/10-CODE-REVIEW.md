# ✅ 10 - Checklist de Code Review

## Guia para Revisão de Código

---

## 1. Checklist Completo

### Funcionalidade
```markdown
- [ ] O código faz o que deveria fazer?
- [ ] Edge cases foram considerados?
- [ ] Null checks implementados?
- [ ] Tratamento de erros adequado?
```

### Clean Code
```markdown
- [ ] Nomes são descritivos?
- [ ] Funções são pequenas (< 30 linhas)?
- [ ] Classes são focadas (< 300 linhas)?
- [ ] Sem código duplicado?
- [ ] Sem números mágicos?
- [ ] SOLID respeitado?
```

### Segurança
```markdown
- [ ] Inputs validados?
- [ ] Nenhum SQL concatenado?
- [ ] Nenhum secret hardcoded?
- [ ] Rate limiting aplicado?
- [ ] Autorização verificada?
```

### Testes
```markdown
- [ ] Testes unitários existem?
- [ ] Testes cobrem sucesso E falha?
- [ ] Mocks configurados corretamente?
- [ ] Cobertura adequada?
```

### Performance
```markdown
- [ ] Queries otimizadas?
- [ ] N+1 queries evitados?
- [ ] Cache apropriado?
- [ ] Async/await correto?
```

### Documentação
```markdown
- [ ] XML docs presentes?
- [ ] Comentários explicam "por quê"?
- [ ] README atualizado?
```

---

## 2. Processo de Review

```
1. PR criado pelo autor
   ↓
2. CI/CD roda automaticamente
   ↓
3. Reviewer atribuído
   ↓
4. Reviewer aplica checklist
   ↓
5. Comentários adicionados (se necessário)
   ↓
6. Autor endereça comentários
   ↓
7. Re-review (se necessário)
   ↓
8. Approval (mínimo 1 para develop, 2 para main)
   ↓
9. Merge (Squash and Merge)
```

---

## 3. Tipos de Comentários

| Prefixo | Significado | Urgência |
|---------|-------------|----------|
| `[BLOCKER]` | Impede merge | 🔴 Crítico |
| `[SUGGESTION]` | Melhoria opcional | 🟡 Baixo |
| `[QUESTION]` | Dúvida/clarificação | ⚪ Neutro |
| `[NIT]` | Nitpick (trivial) | ⚪ Neutro |
| `[SECURITY]` | Problema de segurança | 🔴 Crítico |

### Exemplos

```markdown
[BLOCKER] Este campo não está sendo validado.
A falta de validação permite SQL injection.

[SUGGESTION] Considere extrair esse bloco para um método separado.
Melhoraria a legibilidade.

[QUESTION] Por que usamos singleton aqui ao invés de scoped?

[NIT] Linha muito longa, considere quebrar.

[SECURITY] Esse endpoint não verifica ownership do recurso.
```

---

## 4. Boas Práticas do Reviewer

```markdown
✅ Seja construtivo, não destrutivo
✅ Explique o "por quê" das sugestões
✅ Ofereça soluções, não apenas críticas
✅ Elogie código bom
✅ Responda em tempo hábil (< 24h)
✅ Foque no que importa (não seja pedante)
```

---

## 5. Boas Práticas do Autor

```markdown
✅ PRs pequenos (< 400 linhas)
✅ Descrição clara do que foi feito
✅ Self-review antes de solicitar
✅ Responda comentários educadamente
✅ Não leve críticas pro pessoal
✅ Agradeça pelo feedback
```

---

## 6. Aprovação Rápida

PRs são aprovados rapidamente quando:

```markdown
✅ Pipeline CI/CD passou
✅ Cobertura de testes adequada
✅ Sem código complexo
✅ Segue padrões existentes
✅ Documentação presente
✅ Mudanças de baixo risco
```

---

## 7. Labels de PR

| Label | Uso |
|-------|-----|
| `ready-for-review` | Pronto para revisão |
| `wip` | Work in progress |
| `needs-changes` | Alterações necessárias |
| `approved` | Aprovado |
| `blocked` | Bloqueado por dependência |

---

📅 **Referência:** Google Code Review Guidelines
