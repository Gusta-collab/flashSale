# ADR-003: .NET 8 como Stack Backend

## Status
**Aceito** - 2026-01-09

## Contexto

Precisamos escolher a stack de desenvolvimento para o backend do QueueMaster.
Requisitos:
- Alta performance para processamento assíncrono
- Suporte a Worker Services
- Tipagem forte para código manutenível
- Ecossistema empresarial maduro

## Decisão

Usaremos **.NET 8 (C#)** para API e Worker Services.

## Justificativa

### Por que .NET 8?

1. **Performance**: Um dos runtimes mais rápidos (benchmarks TechEmpower)
2. **Async/Await Nativo**: Ideal para I/O-bound workloads
3. **Worker Service**: Template nativo para background services
4. **Entity Framework Core**: ORM robusto e performático
5. **SignalR**: WebSocket nativo para real-time
6. **Tipagem Forte**: Menos bugs em runtime
7. **LTS**: Suporte até 2026

### Comparação

| Critério | .NET 8 | Node.js | Go | Java |
|----------|--------|---------|-----|------|
| Performance | ✅ Alta | ⚠️ Média | ✅ Alta | ✅ Alta |
| Tipagem | ✅ Forte | ❌ Fraca* | ✅ Forte | ✅ Forte |
| Async | ✅ Nativo | ✅ Nativo | ✅ Nativo | ⚠️ Complexo |
| Worker | ✅ Nativo | ⚠️ Manual | ⚠️ Manual | ⚠️ Spring |
| ORM | ✅ EF Core | ⚠️ Varies | ⚠️ Varies | ✅ Hibernate |

*TypeScript adiciona tipagem, mas é opcional

## Consequências

### Positivas
- Código fortemente tipado = menos bugs
- Performance excelente sem tuning
- Ferramentas integradas (Visual Studio, Rider)
- Execução cross-platform

### Negativas
- Curva de aprendizado para quem vem de linguagens dinâmicas
- Ecossistema open source menor que Node.js

## Referências
- [.NET 8 What's New](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [TechEmpower Benchmarks](https://www.techempower.com/benchmarks/)
