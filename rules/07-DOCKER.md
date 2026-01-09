# 🐳 07 - Docker e Containerização

## Guia de Docker para o Projeto

---

## 1. Dockerfile Multi-stage

```dockerfile
# ════════════════════════════════════════
# STAGE 1: Build
# ════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj primeiro (cache de layers)
COPY ["FlashSale.Api/FlashSale.Api.csproj", "FlashSale.Api/"]
COPY ["FlashSale.Core/FlashSale.Core.csproj", "FlashSale.Core/"]
RUN dotnet restore "FlashSale.Api/FlashSale.Api.csproj"

# Copiar código e buildar
COPY . .
RUN dotnet publish "FlashSale.Api/FlashSale.Api.csproj" \
    -c Release -o /app/publish

# ════════════════════════════════════════
# STAGE 2: Runtime (imagem mínima)
# ════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Usuário não-root (segurança)
RUN adduser -S appuser
WORKDIR /app
COPY --from=build /app/publish .
USER appuser

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s \
  CMD wget -q --spider http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FlashSale.Api.dll"]
```

---

## 2. Docker Compose

```yaml
version: '3.8'

services:
  # ════════════════════════════════════════
  # API
  # ════════════════════════════════════════
  api:
    build: ./src/FlashSale.Api
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=flashsale;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started

  # ════════════════════════════════════════
  # WORKER (3 réplicas)
  # ════════════════════════════════════════
  worker:
    build: ./src/FlashSale.Worker
    deploy:
      replicas: 3
    environment:
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=flashsale;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis

  # ════════════════════════════════════════
  # POSTGRES
  # ════════════════════════════════════════
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: flashsale
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  # ════════════════════════════════════════
  # REDIS
  # ════════════════════════════════════════
  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes
    volumes:
      - redis_data:/data
    ports:
      - "6379:6379"

volumes:
  postgres_data:
  redis_data:
```

---

## 3. Comandos Essenciais

```bash
# Subir ambiente
docker-compose up -d

# Ver logs
docker-compose logs -f api worker

# Escalar workers
docker-compose up -d --scale worker=10

# Rebuild após mudanças
docker-compose up -d --build

# Parar tudo
docker-compose down

# Limpar volumes (CUIDADO!)
docker-compose down -v
```

---

## 4. Boas Práticas

| Prática | Descrição |
|---------|-----------|
| Multi-stage | Imagem final menor |
| Alpine | Imagens base menores |
| Non-root user | Segurança |
| Healthcheck | Monitoramento |
| Layer caching | Build mais rápido |
| .dockerignore | Não copiar desnecessários |

---

## 5. .dockerignore

```dockerignore
**/bin
**/obj
**/.git
**/.vs
**/node_modules
**/*.md
**/Dockerfile*
**/.dockerignore
```

---

📅 **Referência:** Docker Best Practices
