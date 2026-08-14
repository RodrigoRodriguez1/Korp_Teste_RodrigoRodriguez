# 08 — Infraestrutura

## Ambiente local (desenvolvimento)

### Docker Compose — PostgreSQL

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:16-alpine
    container_name: korp_postgres
    environment:
      POSTGRES_USER: korp
      POSTGRES_PASSWORD: korp_dev_2026
      POSTGRES_DB: korp
    ports:
      - "5432:5432"
    volumes:
      - korp_pgdata:/var/lib/postgresql/data
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U korp"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  korp_pgdata:
```

### Script de inicialização

```sql
-- scripts/init.sql
CREATE SCHEMA IF NOT EXISTS estoque;
CREATE SCHEMA IF NOT EXISTS faturamento;
CREATE SEQUENCE IF NOT EXISTS faturamento.nota_fiscal_numero_seq START 1 INCREMENT 1;
```

### Connection strings (Development)

```json
// Estoque.API appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=korp;Username=korp;Password=korp_dev_2026;Search Path=estoque"
  }
}

// Faturamento.API appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=korp;Username=korp;Password=korp_dev_2026;Search Path=faturamento"
  },
  "Services": {
    "EstoqueUrl": "http://localhost:5002"
  }
}
```

### Como rodar localmente

```bash
# 1. Subir banco
docker compose up -d

# 2. Aplicar migrations — Estoque
cd src/backend/Korp.Estoque/Korp.Estoque.API
dotnet ef database update

# 3. Aplicar migrations — Faturamento
cd src/backend/Korp.Faturamento/Korp.Faturamento.API
dotnet ef database update

# 4. Rodar microsserviços (terminais separados)
cd src/backend/Korp.Estoque/Korp.Estoque.API && dotnet run
cd src/backend/Korp.Faturamento/Korp.Faturamento.API && dotnet run

# 5. Rodar Angular
cd src/frontend && ng serve --base-href /korp/
```

---

## Produção

### Banco — Supabase

- Projeto: `korp-teste` (criar no painel do Supabase com o e-mail do GitHub)
- Executar o mesmo `init.sql` via Query Editor do Supabase
- Migrations do EF Core rodam apontando para a connection string do Supabase
- Connection string vem de variáveis de ambiente (nunca no código)

```bash
# Aplicar migrations em produção
DATABASE_URL="Host=<supabase-host>;..." dotnet ef database update
```

### Backend — Render.com (free tier)

Cada microsserviço deployado como Web Service separado no Render:
- `korp-estoque` → `https://korp-estoque.onrender.com`
- `korp-faturamento` → `https://korp-faturamento.onrender.com`

Variáveis de ambiente no painel do Render:
```
ConnectionStrings__DefaultConnection=<supabase-connection-string>
Services__EstoqueUrl=https://korp-estoque.onrender.com  # apenas no Faturamento
ASPNETCORE_ENVIRONMENT=Production
```

> **Por que Render?** Free tier com 750h/mês por serviço, deploy via GitHub push, suporte nativo a .NET, sem cold start em planos pagos (free tier tem cold start de ~30s — aceitável para demonstração).

### Frontend — Cloudflare Pages

- Repositório: `Korp_Teste_RodrigoRodriguez` (branch `main`)
- Build command: `cd src/frontend && npm install && ng build --base-href /korp/ --configuration production`
- Output directory: `src/frontend/dist/frontend/browser`
- Deploy automático a cada push na `main`

### Portfólio — Vercel (existente)

Adicionar ao `vercel.json` do portfólio (em `C:\Users\rodri\Documents\portifolio\skills\portfolio`):

```json
{
  "rewrites": [
    {
      "source": "/korp/:path*",
      "destination": "https://korp-teste-rodrigo.pages.dev/korp/:path*"
    },
    { "source": "/(.*)", "destination": "/index.html" }
  ]
}
```

> A regra `/korp/:path*` deve vir **antes** da regra SPA `/(.*)`

---

## Diagrama de deploy

```
Browser
  │
  ▼
portfolio-rodrigo-rodriguez.vercel.app/korp/*
  │ (rewrite transparente)
  ▼
korp-teste-rodrigo.pages.dev/korp/*  ← Cloudflare Pages (Angular)
  │
  ├─── GET /produtos  ──────────────────► korp-estoque.onrender.com ──► Supabase (schema: estoque)
  │
  └─── POST /notas-fiscais/{id}/imprimir ► korp-faturamento.onrender.com
                                              │ (chama internamente)
                                              └─► korp-estoque.onrender.com ──► Supabase (schema: faturamento)
```

---

## Variáveis de ambiente por contexto

| Variável | Local | Produção |
|----------|-------|----------|
| `ConnectionStrings__DefaultConnection` | docker postgres | Supabase URL |
| `Services__EstoqueUrl` (Faturamento) | `http://localhost:5002` | `https://korp-estoque.onrender.com` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `ASPNETCORE_URLS` | `http://localhost:5001` / `5002` | gerenciado pelo Render |

Nenhuma variável de ambiente está no código ou no repositório. Arquivo `.env.example` no raiz documenta o que é necessário.
