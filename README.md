# Korp ERP — Sistema de Emissão de Notas Fiscais

Desafio técnico para a vaga de Desenvolvedor Web Fullstack (C# + Angular) na **Korp ERP by Viasoft**.

## Demonstração

Acesse o sistema em produção:
> **[portfolio-rodrigo-rodriguez.vercel.app/korp](https://portfolio-rodrigo-rodriguez.vercel.app/korp)**

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Frontend | Angular 21 · TypeScript · Angular Material · RxJS |
| Backend | .NET 9 · ASP.NET Core Minimal API · Clean Architecture · CQRS (MediatR) |
| Banco de dados | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Validação | FluentValidation |
| Resiliência | Polly (retry + circuit breaker) |
| Dev local | Docker Compose |
| Produção banco | Supabase |
| Produção backend | Render.com |
| Produção frontend | Cloudflare Pages |

## Arquitetura

Dois microsserviços independentes:

- **Estoque.API** (porta 5002) — controle de produtos e saldos
- **Faturamento.API** (porta 5001) — gestão de notas fiscais, orquestra a impressão

```
Angular → Faturamento.API → Estoque.API → PostgreSQL
                         ↘               ↗
                           (Polly retry)
```

## Como rodar localmente

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 22+](https://nodejs.org)
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`

### 1. Subir o banco de dados

```bash
docker compose up -d
```

Aguarde o healthcheck passar (~5s). O PostgreSQL sobe na porta `5432` com os schemas `estoque` e `faturamento` criados automaticamente.

### 2. Aplicar migrations

```bash
# Estoque
cd src/backend/Korp.Estoque/Korp.Estoque.API
dotnet ef database update

# Faturamento
cd ../../../Korp.Faturamento/Korp.Faturamento.API
dotnet ef database update
```

### 3. Rodar os microsserviços

Em terminais separados:

```bash
# Terminal 1 — Estoque.API (porta 5002)
cd src/backend/Korp.Estoque/Korp.Estoque.API
dotnet run

# Terminal 2 — Faturamento.API (porta 5001)
cd src/backend/Korp.Faturamento/Korp.Faturamento.API
dotnet run
```

### 4. Rodar o frontend

```bash
cd src/frontend
npm install
ng serve
```

Acesse: [http://localhost:4200/korp](http://localhost:4200/korp)

---

## Como simular falha do Estoque

Para demonstrar o tratamento de falhas:

1. Com o sistema rodando, acesse uma Nota Fiscal com status **Aberta**
2. Pare o Estoque.API (Ctrl+C no terminal correspondente)
3. Clique em **Imprimir Nota** no Angular
4. O sistema exibe: *"Serviço de estoque temporariamente indisponível. Sua nota não foi alterada."*
5. A NF permanece com status **Aberta**
6. Suba o Estoque.API novamente e tente imprimir — funciona normalmente

O Faturamento.API usa **Polly** com 3 retries e circuit breaker antes de retornar 503.

---

## Rodar testes

```bash
# Testes unitários
dotnet test tests/Korp.Estoque.UnitTests
dotnet test tests/Korp.Faturamento.UnitTests
```

---

## Decisões técnicas

Documentação completa em [`docs/specs/`](docs/specs/):

| Documento | Conteúdo |
|-----------|----------|
| [01 — Arquitetura](docs/specs/01-architecture.md) | Microsserviços, camadas, padrões |
| [02 — Estrutura](docs/specs/02-project-structure.md) | Organização de pastas e projetos |
| [03 — Regras de desenvolvimento](docs/specs/03-development-rules.md) | Convenções, SOLID, async |
| [04 — Modelo de dados](docs/specs/04-data-model.md) | Tabelas, entidades, relacionamentos |
| [05 — Contratos de API](docs/specs/05-api-contracts.md) | Endpoints, request/response |
| [06 — Regras de negócio](docs/specs/06-business-rules.md) | Fluxo de impressão, validações |
| [07 — Frontend](docs/specs/07-frontend-architecture.md) | Angular, telas, ciclos de vida, RxJS |
| [08 — Infraestrutura](docs/specs/08-infrastructure.md) | Docker, Supabase, Render, deploy |
| [09 — Testes](docs/specs/09-testing-strategy.md) | Estratégia, exemplos, casos obrigatórios |
| [10 — Critérios de aceite](docs/specs/10-acceptance-criteria.md) | Checklists por etapa |

---

## Funcionalidades implementadas

- [x] Cadastro de Produtos (Código, Descrição, Saldo)
- [x] Cadastro de Notas Fiscais (numeração sequencial, múltiplos produtos)
- [x] Impressão de NF (fecha nota + desconta saldo atomicamente)
- [x] Arquitetura de Microsserviços (Estoque + Faturamento)
- [x] Tratamento de falhas com Polly (retry + circuit breaker)
- [x] Banco de dados real (PostgreSQL)
- [x] Tratamento de concorrência (pessimistic lock via `SELECT FOR UPDATE`)
- [x] Idempotência na impressão (`Idempotency-Key` header)

---

*Rodrigo Rodriguez — rodrigorfig1@gmail.com*
