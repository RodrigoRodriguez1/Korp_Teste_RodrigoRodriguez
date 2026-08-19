# Detalhamento Técnico — Korp ERP

Respostas às perguntas do desafio técnico.

---

## 1. Ciclos de vida do Angular utilizados

| Hook | Onde | Para quê |
|------|------|----------|
| `ngOnInit` | `ProdutosListaComponent`, `ProdutoFormComponent`, `NotasFiscaisListaComponent`, `NotasFiscaisDetalheComponent`, `NotasFiscaisCriarComponent` | Disparar carregamento de dados da API assim que o componente é montado |

O projeto usa **Angular 21 Standalone Components**. O ciclo de vida é complementado por duas APIs modernas do Angular que substituem padrões antigos:

- **`input()`** (signal-based input) — em vez de `@Input()`, usado em `NotasFiscaisDetalheComponent` para receber o `id` da rota: `readonly id = input.required<string>()`
- **`signal()`** — estado local reativo (loading, lista de itens, nota carregada) em todos os componentes de feature, sem necessidade de `ngOnChanges` ou `ChangeDetectorRef`
- **`toSignal()`** — converte um `Observable` em `Signal` no `ShellComponent` para observar breakpoints de responsividade

---

## 2. RxJS — como foi utilizado

Sim, RxJS é usado extensivamente nos serviços HTTP e na comunicação entre componentes.

### Nos serviços

Os serviços (`ProdutoService`, `NotaFiscalService`) retornam `Observable<T>` diretamente do `HttpClient` sem transformação adicional. Os componentes fazem `.subscribe()` e atualizam signals:

```typescript
// ProdutoService
getAll(): Observable<Produto[]> {
  return this.http.get<Produto[]>(this.baseUrl);
}

// No componente
this.service.getAll().subscribe({
  next: (data) => this.produtos.set(data),
  complete: () => this.loading.set(false),
});
```

### Operadores utilizados

| Operador | Onde | Para quê |
|----------|------|----------|
| `catchError` | `error.interceptor.ts` (global) | Intercepta todos os erros HTTP e exibe `MatSnackBar` com a mensagem do `ProblemDetails` |
| `throwError` | `error.interceptor.ts` | Repropaga o erro após exibir a notificação |
| `map` | `shell.component.ts` | Mapeia resultado do `BreakpointObserver` para boolean de responsividade |
| `toSignal` | `shell.component.ts` | Converte Observable do breakpoint em Signal para uso no template |

### Interceptor global de erros

O `errorInterceptor` é registrado no `app.config.ts` e trata centralizadamente todos os erros HTTP, diferenciando 503 (Estoque offline), 4xx (validação/negócio) e 5xx (erro interno), exibindo mensagens do campo `detail` do ProblemDetails:

---

## 3. Bibliotecas utilizadas

### Frontend (Angular)

| Biblioteca | Versão | Finalidade |
|-----------|--------|-----------|
| `@angular/material` | 21 | Componentes visuais (tabela, formulários, diálogos, snackbar) |
| `@angular/cdk` | 21 | Base para Angular Material |
| `rxjs` | 7 | Programação reativa (HttpClient, streams) |

### Backend (.NET)

| Biblioteca | Versão | Finalidade |
|-----------|--------|-----------|
| `MediatR` | 12 | Implementar CQRS (Commands, Queries, Pipeline Behaviors) |
| `FluentValidation` | 11 | Validação declarativa e testável de Commands |
| `Polly` | 8 | Retry com backoff exponencial + circuit breaker |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | Provider EF Core para PostgreSQL |
| `Microsoft.EntityFrameworkCore` | 9 | ORM Code First, migrations |

---

## 4. Componentes visuais (Angular Material)

| Componente | Onde utilizado |
|-----------|---------------|
| `MatToolbar` | Navbar de navegação global (`ShellComponent`) |
| `MatTable` | Lista de produtos e lista de NFs |
| `MatButton` / `MatIconButton` | Ações (criar, editar, excluir, imprimir) |
| `MatFormField` + `MatInput` | Formulários de produto e criação de NF |
| `MatSelect` | Seleção de produto na tela de criar NF |
| `MatDialog` | `ConfirmDialogComponent` (exclusão) e `ImprimirDialogComponent` (impressão com spinner) |
| `MatSnackBar` | Feedback global de sucesso e erro (via interceptor e componentes) |
| `MatProgressSpinner` | Indicador de carregamento e processamento da impressão |
| `MatChips` | Badge de status da NF (`Aberta` = verde / `Fechada` = cinza) |
| `MatCard` | Container do detalhe da NF |
| `MatTooltip` | Dicas nos botões de ação da lista de produtos |
| `MatSidenav` | Menu lateral responsivo no shell |

---

## 5. Frameworks e padrões C#

### ASP.NET Core 9 — Minimal API

Escolhida em vez de Controllers para reduzir boilerplate. Os endpoints são registrados via extension methods:

```csharp
app.MapGroup("/produtos").WithTags("Produtos")
   .MapGet("/", GetAll)
   .MapPost("/", Create);
```

### Clean Architecture

Cada microsserviço tem quatro camadas com dependências unidirecionais:

```
Domain ← Application ← Infrastructure ← API
```

- **Domain:** entidades (`Produto`, `NotaFiscal`), regras de negócio puras, interfaces de repositório
- **Application:** Commands, Queries, Handlers (CQRS), DTOs, validações (FluentValidation)
- **Infrastructure:** EF Core DbContext, repositórios, HttpClient (chamadas ao Estoque)
- **API:** `Program.cs`, endpoints, middlewares, DI

### CQRS com MediatR

Cada operação é um objeto imutável (`record`):

```csharp
public sealed record CreateProdutoCommand(string Codigo, string Descricao, int Saldo)
    : IRequest<Result<ProdutoDto>>;
```

O `ValidationBehavior<TRequest, TResponse>` no pipeline do MediatR executa FluentValidation antes de qualquer handler chegar.

### Result Pattern

Handlers nunca lançam exceção para erros de negócio:

```csharp
public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static Error NotFound(string desc) => new("NotFound", desc, ErrorType.NotFound);
    public static Error Conflict(string desc) => new("Conflict", desc, ErrorType.Conflict);
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
}
```

A camada API converte `Result<T>` em `IResult` do Minimal API (200/400/404/409/503).

### ProblemDetails (RFC 7807)

Todos os erros retornam o formato padronizado:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Saldo insuficiente",
  "status": 409,
  "detail": "Produto 'PROD-001' possui saldo 1, mas a nota requer 2 unidades."
}
```

---

## 6. Tratamento de erros e exceções no backend

### Três camadas de tratamento

**1. Erros de validação (FluentValidation)** — capturados pelo `ValidationBehavior` antes de qualquer handler. Retornam 400 com a lista de campos inválidos.

**2. Erros de negócio (Result Pattern)** — handlers retornam `Result<T>` com `Error` tipado. A camada API converte para o status HTTP correto via `switch` no `ErrorType`.

**3. Exceções inesperadas (Middleware global)** — `ExceptionMiddleware` captura qualquer exceção não tratada, loga com `ILogger`, e retorna 500 com ProblemDetails sem expor stack trace em produção.

```
Request → ValidationBehavior → Handler → Result<T>
                                              ↓
                              Error → ToProblem() → 400/404/409/503
                              Exception → ExceptionMiddleware → 500
```

### Resiliência entre microsserviços (Polly)

O `HttpClient` que o Faturamento.API usa para chamar o Estoque.API tem três políticas encadeadas:

```csharp
// Timeout por tentativa
Policy.TimeoutAsync(TimeSpan.FromSeconds(5))

// Retry: 3x com backoff exponencial (1s, 2s, 4s)
Policy.Handle<HttpRequestException>()
      .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)))

// Circuit Breaker: 5 falhas → abre por 30s
Policy.Handle<HttpRequestException>()
      .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
```

Quando o Estoque retorna erro ou está offline, o Faturamento captura e retorna 503 — a NF permanece `Aberta` e nenhum saldo é alterado.

---

## 7. LINQ — como foi utilizado

LINQ é usado nas queries de leitura e nas operações de validação. Exemplos reais do código:

```csharp
// Listar produtos ordenados por código
await _context.Produtos
    .AsNoTracking()
    .OrderBy(p => p.Codigo)
    .ToListAsync(cancellationToken);

// Buscar por ID
await _context.Produtos
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

// Buscar por código (validação de duplicidade)
await _context.Produtos
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

// Listar NFs com itens (eager loading)
await _context.NotasFiscais
    .AsNoTracking()
    .Include(n => n.Itens)
    .OrderByDescending(n => n.Numero)
    .ToListAsync(cancellationToken);
```

No Application layer, LINQ é usado para projeção de entidades em DTOs:

```csharp
var dto = new NotaFiscalDto(
    nota.Id,
    nota.Numero,
    nota.Status.ToString(),
    nota.ImpressoEm,
    nota.Itens.Select(i => new ItemNotaFiscalDto(
        i.Id, i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade
    )).ToList(),
    nota.CriadoEm
);
```

---

## 8. Concorrência — como foi tratada

### Problema

Dois requests simultâneos tentando imprimir NFs que usam o mesmo produto podem causar race condition: os dois leem o saldo disponível (ex: 5 unidades), os dois aprovam, e o saldo fica negativo após ambos descontarem.

### Solução: Pessimistic Locking (SELECT FOR UPDATE)

O endpoint `POST /produtos/descontar-saldo` executa dentro de uma **transação de banco de dados** com lock exclusivo nas linhas:

```csharp
return await _context.Produtos
    .FromSqlRaw(
        "SELECT * FROM korp_estoque.produtos WHERE id = ANY(@p0) FOR UPDATE",
        new NpgsqlParameter("p0", idList.ToArray()))
    .ToListAsync(cancellationToken);
```

O `FOR UPDATE` faz o PostgreSQL adquirir um lock exclusivo nas linhas selecionadas. Se dois requests chegam simultaneamente:

1. Request A adquire o lock, lê saldo = 5, desconta 3 → saldo = 2, faz COMMIT, libera lock
2. Request B aguardava o lock, agora lê saldo = 2, verifica se tem o suficiente e prossegue (ou retorna 409 se não tiver)

Nunca haverá saldo negativo.

---

## 9. Idempotência — como foi implementada

### Problema

Se o Angular envia o request de impressão e a rede cai antes da resposta, ele pode reenviar — resultando em duplo desconto de saldo.

### Solução

O Angular gera um UUID v4 antes de cada impressão e envia no header `Idempotency-Key`. O Faturamento.API:

1. Verifica se a chave já existe na tabela `korp_faturamento.idempotency_keys`
2. Se existe: retorna a resposta original armazenada (sem reprocessar)
3. Se não existe: processa normalmente e armazena a resposta com TTL de 24h

```sql
-- Tabela de idempotência
CREATE TABLE korp_faturamento.idempotency_keys (
    key        TEXT PRIMARY KEY,
    response   JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    expires_at TIMESTAMPTZ DEFAULT NOW() + INTERVAL '24 hours'
);
```

Isso garante que reenvios (por double-click, timeout de rede, retry do Polly) nunca resultam em duplo desconto.

---

## 10. Decisões arquiteturais e trade-offs

| Decisão | Escolha | Alternativa descartada | Motivo |
|---------|---------|----------------------|--------|
| API style | Minimal API | MVC Controllers | Menos boilerplate para endpoints simples |
| CQRS | MediatR | Serviços diretos | Pipeline behaviors (validação) sem acoplamento |
| Sequencial NF | PostgreSQL SEQUENCE | `MAX(numero)+1` | Thread-safe sem necessidade de lock |
| Schemas | `korp_estoque` / `korp_faturamento` no mesmo banco | Dois bancos separados | Simplicidade no free tier do Supabase |
| Lock | `SELECT FOR UPDATE` | Optimistic concurrency | Saldo negativo é inaceitável — preferível bloquear |
| Deploy backend | Render.com | Railway, Fly.io | Suporte a Docker, Blueprint YAML, free tier |
| Deploy frontend | Cloudflare Pages | Vercel, Netlify | Melhor integração com `wrangler`, CDN global |

---

*Rodrigo Rodriguez — rodrigorfig1@gmail.com*
