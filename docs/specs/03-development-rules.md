# 03 — Regras de Desenvolvimento

## Gerais

- Código em **inglês** (variáveis, classes, métodos, arquivos)
- Comentários e documentação em **português**
- Sem código comentado no repositório — se não está sendo usado, não existe
- Sem `TODO` sem issue associada

## .NET / C#

### Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Classes, interfaces, records | PascalCase | `NotaFiscal`, `IProdutoRepository` |
| Métodos | PascalCase | `GetAllAsync`, `DescontarSaldo` |
| Parâmetros e variáveis locais | camelCase | `produtoId`, `notaFiscal` |
| Constantes | PascalCase | `MaxQuantidade` |
| Campos privados | _camelCase | `_repository`, `_mediator` |
| Interfaces | prefixo `I` | `INotaFiscalRepository` |
| Commands/Queries | sufixo Command/Query | `CreateProdutoCommand` |
| Handlers | sufixo CommandHandler/QueryHandler | `CreateProdutoCommandHandler` |
| Validators | sufixo Validator | `CreateProdutoCommandValidator` |
| DTOs | sufixo Dto ou Request/Response | `ProdutoDto`, `CreateProdutoRequest` |

### SOLID

- **Single Responsibility:** cada classe tem uma única razão para mudar. Handler cuida de um único caso de uso.
- **Open/Closed:** novos comportamentos via novos handlers, não via modificação de handlers existentes.
- **Liskov Substitution:** implementações de repositório devem ser substituíveis pela interface.
- **Interface Segregation:** `IProdutoRepository` expõe apenas o que o domínio precisa, não métodos de infraestrutura.
- **Dependency Inversion:** handlers dependem de `IProdutoRepository`, nunca de `ProdutoRepository`.

### Async/Await

- Todo método I/O é `async` e retorna `Task<T>` — sem exceção
- `CancellationToken` obrigatório em todos os métodos de repositório e handlers
- Proibido: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`

### Entity Framework Core

- `AsNoTracking()` em todas as queries de leitura
- `IEntityTypeConfiguration<T>` por entidade — sem configuração no `OnModelCreating` inline
- Migrations geradas via CLI: `dotnet ef migrations add <NomeMigration> --project Infrastructure --startup-project API`
- Nunca `SaveChanges()` — sempre `SaveChangesAsync(cancellationToken)`
- Índices explícitos para: `Produto.Codigo` (único), `NotaFiscal.Numero` (único)

### Result Pattern

```csharp
// CORRETO — erros de negócio retornam Result
public async Task<Result<ProdutoDto>> Handle(GetProdutoByIdQuery query, CancellationToken ct)
{
    var produto = await _repository.GetByIdAsync(query.Id, ct);
    if (produto is null)
        return Result.Failure<ProdutoDto>(ProdutoErrors.NotFound(query.Id));
    return Result.Success(_mapper.Map<ProdutoDto>(produto));
}

// ERRADO — não lançar exceção para fluxo de negócio
throw new NotFoundException("Produto não encontrado");
```

### Tratamento de erros na API

- Exception Middleware global captura exceções inesperadas e retorna `ProblemDetails` (RFC 7807) com status 500
- Erros de negócio (Result.IsFailure) são convertidos em respostas HTTP semânticas pela camada API
- Nunca expor stack trace em produção

### Validação

```csharp
// FluentValidation — sempre no Application, nunca no Domain
public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
    public CreateProdutoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Saldo).GreaterThanOrEqualTo(0);
    }
}
```

### Injeção de Dependência

- Sempre via construtor — sem `ServiceLocator`, sem `IServiceProvider` injetado em classes de negócio
- Extension methods `AddApplication()` e `AddInfrastructure()` por projeto
- Repositórios registrados como `Scoped`
- MediatR registrado no `Application`
- HttpClient do Estoque registrado com `AddHttpClient` + Polly no `Infrastructure` do Faturamento

### LINQ

- Usar LINQ para filtros, projeções e ordenações em queries EF Core
- Evitar LINQ para lógica de negócio complexa — preferir métodos explícitos
- Projeção com `.Select()` nos `QueryHandlers` de leitura — nunca retornar entidade de domínio

```csharp
// CORRETO
var dtos = await _context.Produtos
    .AsNoTracking()
    .Where(p => !p.IsDeleted)
    .Select(p => new ProdutoDto(p.Id, p.Codigo, p.Descricao, p.Saldo))
    .ToListAsync(cancellationToken);
```

### Concorrência (opcional implementado)

- Controle via **Pessimistic Locking** no PostgreSQL: `FOR UPDATE` na query de desconto de saldo
- EF Core: `ExecuteSqlRawAsync` com `SELECT ... FOR UPDATE` dentro de transação
- Garante que dois requests simultâneos não ultrapassem o saldo disponível

### Idempotência (opcional implementado)

- Header `Idempotency-Key` (UUID) nos requests de impressão de NF
- Faturamento.API armazena o par `(idempotency_key, response)` na tabela `idempotency_keys`
- Se mesma chave chega novamente, retorna a resposta cacheada sem reprocessar

## Angular / TypeScript

### Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Componentes | PascalCase + sufixo Component | `ProdutosListComponent` |
| Serviços | PascalCase + sufixo Service | `ProdutoService` |
| Interfaces/Models | PascalCase | `Produto`, `NotaFiscal` |
| Arquivos | kebab-case | `produtos-list.component.ts` |
| Rotas | kebab-case | `/produtos`, `/notas-fiscais` |
| Signals | camelCase | `produtos = signal<Produto[]>([])` |
| Observables | sufixo $ | `produtos$` |

### Padrões obrigatórios

- **Standalone Components** — sem NgModules
- **Signals** para estado local de componente
- **RxJS** para streams HTTP (`HttpClient` retorna `Observable`)
- **ReactiveFormsModule** para todos os formulários
- **OnPush** change detection em componentes presentacionais
- `trackBy` obrigatório em todos os `@for` / `*ngFor`
- `AsyncPipe` para subscrições em template

### Sem `any`

```typescript
// ERRADO
getData(): any { }

// CORRETO
getData(): Observable<Produto[]> { }
```

### Ciclos de vida usados

| Hook | Onde | Para quê |
|------|------|---------|
| `ngOnInit` | Containers | Carregar dados iniciais via service |
| `ngOnDestroy` | Containers com subscriptions manuais | Cancelar subscriptions com `takeUntilDestroyed()` |
| `ngAfterViewInit` | Componentes com ViewChild | Acessar elemento DOM após renderização |

### Tratamento de erros HTTP

- `ErrorInterceptor` global: captura erros HTTP, exibe toast com mensagem amigável
- Mensagens mapeadas por status: 400 → "Dados inválidos", 409 → "Conflito de estoque", 503 → "Serviço de estoque indisponível"
- Spinner global controlado via `signal<boolean>` no serviço de loading

## Git

### Commits (Conventional Commits)

```
feat(estoque): add create produto endpoint
fix(faturamento): prevent printing closed nota fiscal
chore(infra): add docker compose for postgresql
docs(specs): add api contracts specification
test(estoque): add unit tests for descontar saldo handler
```

### Estratégia de branches

```
main          ← produção (protegida)
develop       ← integração
feat/estoque  ← feature branch por microsserviço
feat/faturamento
feat/frontend
```

### Critério para merge

- Sem erros de build (`dotnet build` / `ng build`)
- Testes passando (`dotnet test`)
- Sem warnings de nullable reference
