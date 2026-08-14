# 01 — Arquitetura

## Visão geral

Sistema de emissão de Notas Fiscais com arquitetura de microsserviços, frontend Angular e dois backends .NET independentes comunicando-se via HTTP.

```
┌─────────────────────────────────────────────────────────────┐
│  Browser — Angular 17                                       │
│  portfolio-rodrigo-rodriguez.vercel.app/korp                │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP (REST + JSON)
          ┌──────────┴──────────┐
          │                     │
┌─────────▼──────────┐ ┌───────▼────────────┐
│  Faturamento.API   │ │   Estoque.API       │
│  .NET 9 / Minimal  │ │   .NET 9 / Minimal  │
│  Porta 5001        │ │   Porta 5002        │
└─────────┬──────────┘ └───────┬────────────┘
          │ HTTP interno        │
          └──────────┬──────────┘
                     │ (Faturamento chama Estoque ao imprimir NF)
          ┌──────────▼──────────┐
          │   PostgreSQL        │
          │   2 schemas:        │
          │   • faturamento     │
          │   • estoque         │
          └─────────────────────┘
```

## Microsserviços

### Estoque.API
- **Responsabilidade:** CRUD de produtos, controle de saldo
- **Banco:** schema `estoque` no PostgreSQL
- **Porta local:** 5002
- **Expõe:** endpoints de produto + endpoint interno de desconto de saldo (chamado pelo Faturamento)

### Faturamento.API
- **Responsabilidade:** CRUD de notas fiscais, orquestração da impressão
- **Banco:** schema `faturamento` no PostgreSQL
- **Porta local:** 5001
- **Consome:** Estoque.API via HttpClient com Polly (retry + circuit breaker)
- **Orquestra:** ao imprimir NF → chama Estoque para descontar saldo → fecha NF

## Camadas internas (cada microsserviço)

```
MicroServiço/
├── Domain/           # Entidades, Value Objects, interfaces, erros de domínio
├── Application/      # Use Cases (CQRS), Commands, Queries, DTOs, validações
├── Infrastructure/   # EF Core, repositórios, HttpClient para outros serviços
└── API/              # Program.cs, Minimal API endpoints, middlewares
```

### Responsabilidades por camada

| Camada | Contém | Depende de |
|--------|--------|------------|
| Domain | Entidades, regras de negócio puras, interfaces de repositório | Nada |
| Application | Use Cases, Commands/Queries, DTOs, FluentValidation | Domain |
| Infrastructure | EF Core DbContext, repositórios, HttpClient | Domain, Application |
| API | Minimal API routes, middlewares, DI setup | Application, Infrastructure |

## Padrões arquiteturais

### CQRS com MediatR
- `Command` + `CommandHandler` para escrita (criar produto, criar NF, imprimir NF)
- `Query` + `QueryHandler` para leitura (listar produtos, listar NFs, buscar por ID)
- Validação via `ValidationBehavior<TRequest, TResponse>` no pipeline do MediatR

### Result Pattern
- Todos os handlers retornam `Result<T>` — nunca lançam exceção para erros de negócio
- Erros tipados: `Error.NotFound`, `Error.Validation`, `Error.Conflict`, `Error.ServiceUnavailable`
- A camada API converte `Result<T>` em `IResult` do Minimal API (200/400/404/409/503)

### Tratamento de falha entre serviços
- Faturamento.API usa Polly: 3 retries com exponential backoff ao chamar Estoque.API
- Circuit breaker: após 5 falhas consecutivas, abre o circuito por 30s
- Quando Estoque indisponível → Faturamento retorna `503 Service Unavailable` com ProblemDetails
- Angular exibe mensagem amigável (não trava a tela, não perde dados da NF)

## Comunicação entre serviços

```
Angular → POST /notas/{id}/imprimir (Faturamento.API)
Faturamento.API → POST /internal/produtos/descontar-saldo (Estoque.API)
Estoque.API → 200 OK | 409 Conflict (saldo insuficiente) | 503 (offline)
Faturamento.API → fecha NF se Estoque respondeu 200
Faturamento.API → retorna erro se Estoque falhou (NF permanece Aberta)
```

## Decisões técnicas e trade-offs

| Decisão | Escolha | Alternativa descartada | Motivo |
|---------|---------|----------------------|--------|
| ORM | EF Core 9 (Code First) | Dapper | Migrations automáticas, menos boilerplate para CRUD |
| CQRS | MediatR 12 | Manual | Pipeline behaviors (validação, logging) sem acoplamento |
| Validação | FluentValidation | DataAnnotations | Regras complexas legíveis, testáveis isoladamente |
| Resiliência | Polly 8 | Retry manual | Circuit breaker nativo, integrado ao HttpClientFactory |
| Banco local | Docker Compose | Instância local | Reproduzível, sem conflito com outras versões |
| Banco produção | Supabase | Railway / Render | Já tem conta, PostgreSQL gerenciado, free tier 500MB |
| Sequencial NF | PostgreSQL SEQUENCE | MAX(numero)+1 | Thread-safe, sem race condition |
| Schema | 2 schemas no mesmo PG | 2 bancos separados | Simplicidade no free tier do Supabase |
