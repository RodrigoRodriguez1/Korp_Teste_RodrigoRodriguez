# 02 — Estrutura do Projeto

## Repositório raiz

```
Korp_Teste_RodrigoRodriguez/
├── src/
│   ├── frontend/                        # Angular 17+
│   └── backend/
│       ├── Korp.SharedKernel/           # Biblioteca compartilhada (Result<T>, erros)
│       ├── Korp.Estoque/                # Microsserviço de Estoque
│       └── Korp.Faturamento/            # Microsserviço de Faturamento
├── docs/
│   └── specs/                           # Specifications (este diretório)
├── docker-compose.yml                   # PostgreSQL local
├── docker-compose.override.yml          # Overrides locais (portas, volumes)
├── Korp.sln                             # Solution .NET agregando todos os projetos
└── README.md
```

## Backend — estrutura detalhada

### SharedKernel (biblioteca, não microsserviço)

```
Korp.SharedKernel/
├── Korp.SharedKernel.csproj
├── Results/
│   ├── Result.cs                        # Result<T> e Result (sem valor)
│   └── Error.cs                         # Tipos de erro: NotFound, Validation, Conflict, etc.
└── Contracts/
    └── ICurrentUserService.cs           # Interface para user claims (se auth for adicionada)
```

### Estoque.API

```
Korp.Estoque/
├── Korp.Estoque.Domain/
│   ├── Korp.Estoque.Domain.csproj
│   ├── Entities/
│   │   └── Produto.cs
│   ├── Repositories/
│   │   └── IProdutoRepository.cs
│   └── Errors/
│       └── ProdutoErrors.cs             # Error.NotFound("Produto não encontrado"), etc.
│
├── Korp.Estoque.Application/
│   ├── Korp.Estoque.Application.csproj
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   └── Mappings/
│   │       └── ProdutoMappings.cs
│   ├── Products/
│   │   ├── Commands/
│   │   │   ├── CreateProduto/
│   │   │   │   ├── CreateProdutoCommand.cs
│   │   │   │   ├── CreateProdutoCommandHandler.cs
│   │   │   │   └── CreateProdutoCommandValidator.cs
│   │   │   ├── UpdateProduto/
│   │   │   │   ├── UpdateProdutoCommand.cs
│   │   │   │   ├── UpdateProdutoCommandHandler.cs
│   │   │   │   └── UpdateProdutoCommandValidator.cs
│   │   │   ├── DeleteProduto/
│   │   │   │   ├── DeleteProdutoCommand.cs
│   │   │   │   └── DeleteProdutoCommandHandler.cs
│   │   │   └── DescontarSaldo/
│   │   │       ├── DescontarSaldoCommand.cs
│   │   │       ├── DescontarSaldoCommandHandler.cs
│   │   │       └── DescontarSaldoCommandValidator.cs
│   │   ├── Queries/
│   │   │   ├── GetAllProdutos/
│   │   │   │   ├── GetAllProdutosQuery.cs
│   │   │   │   └── GetAllProdutosQueryHandler.cs
│   │   │   └── GetProdutoById/
│   │   │       ├── GetProdutoByIdQuery.cs
│   │   │       └── GetProdutoByIdQueryHandler.cs
│   │   └── DTOs/
│   │       ├── ProdutoDto.cs
│   │       └── DescontarSaldoDto.cs
│
├── Korp.Estoque.Infrastructure/
│   ├── Korp.Estoque.Infrastructure.csproj
│   ├── Persistence/
│   │   ├── EstoqueDbContext.cs
│   │   ├── Configurations/
│   │   │   └── ProdutoConfiguration.cs  # IEntityTypeConfiguration<Produto>
│   │   ├── Migrations/                  # Geradas pelo EF CLI
│   │   └── Repositories/
│   │       └── ProdutoRepository.cs
│   └── DependencyInjection.cs           # Extension method AddInfrastructure()
│
└── Korp.Estoque.API/
    ├── Korp.Estoque.API.csproj
    ├── Program.cs
    ├── Endpoints/
    │   └── ProdutoEndpoints.cs          # MapGroup("/produtos")
    ├── Middlewares/
    │   └── ExceptionMiddleware.cs
    └── appsettings.json / appsettings.Development.json
```

### Faturamento.API

```
Korp.Faturamento/
├── Korp.Faturamento.Domain/
│   ├── Entities/
│   │   ├── NotaFiscal.cs
│   │   └── ItemNota.cs
│   ├── Repositories/
│   │   └── INotaFiscalRepository.cs
│   ├── Enums/
│   │   └── StatusNota.cs               # Aberta = 1, Fechada = 2
│   └── Errors/
│       └── NotaFiscalErrors.cs
│
├── Korp.Faturamento.Application/
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   └── ExternalServices/
│   │       └── IEstoqueService.cs      # Interface para o cliente HTTP do Estoque
│   └── NotasFiscais/
│       ├── Commands/
│       │   ├── CreateNotaFiscal/
│       │   │   ├── CreateNotaFiscalCommand.cs
│       │   │   ├── CreateNotaFiscalCommandHandler.cs
│       │   │   └── CreateNotaFiscalCommandValidator.cs
│       │   └── ImprimirNotaFiscal/
│       │       ├── ImprimirNotaFiscalCommand.cs
│       │       ├── ImprimirNotaFiscalCommandHandler.cs
│       │       └── ImprimirNotaFiscalCommandValidator.cs  (idempotência aqui)
│       ├── Queries/
│       │   ├── GetAllNotasFiscais/
│       │   └── GetNotaFiscalById/
│       └── DTOs/
│           ├── NotaFiscalDto.cs
│           ├── CreateNotaFiscalRequest.cs
│           └── ItemNotaDto.cs
│
├── Korp.Faturamento.Infrastructure/
│   ├── Persistence/
│   │   ├── FaturamentoDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── NotaFiscalConfiguration.cs
│   │   │   └── ItemNotaConfiguration.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       └── NotaFiscalRepository.cs
│   ├── ExternalServices/
│   │   └── EstoqueHttpService.cs       # Implementa IEstoqueService com HttpClient + Polly
│   └── DependencyInjection.cs
│
└── Korp.Faturamento.API/
    ├── Program.cs
    ├── Endpoints/
    │   └── NotaFiscalEndpoints.cs
    ├── Middlewares/
    │   └── ExceptionMiddleware.cs
    └── appsettings.json
```

## Frontend — estrutura detalhada

```
frontend/                                # Angular 17+ standalone
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/
│   │   │   │   ├── produto.service.ts
│   │   │   │   └── nota-fiscal.service.ts
│   │   │   ├── interceptors/
│   │   │   │   └── error.interceptor.ts
│   │   │   └── models/
│   │   │       ├── produto.model.ts
│   │   │       └── nota-fiscal.model.ts
│   │   ├── features/
│   │   │   ├── produtos/
│   │   │   │   ├── produtos-list/
│   │   │   │   │   ├── produtos-list.component.ts
│   │   │   │   │   └── produtos-list.component.html
│   │   │   │   ├── produto-form/
│   │   │   │   │   ├── produto-form.component.ts
│   │   │   │   │   └── produto-form.component.html
│   │   │   │   └── produtos.routes.ts
│   │   │   └── notas-fiscais/
│   │   │       ├── notas-list/
│   │   │       │   ├── notas-list.component.ts
│   │   │       │   └── notas-list.component.html
│   │   │       ├── nota-form/
│   │   │       │   ├── nota-form.component.ts
│   │   │       │   └── nota-form.component.html
│   │   │       ├── nota-detail/
│   │   │       │   ├── nota-detail.component.ts
│   │   │       │   └── nota-detail.component.html
│   │   │       └── notas-fiscais.routes.ts
│   │   ├── shared/
│   │   │   ├── components/
│   │   │   │   ├── loading-spinner/
│   │   │   │   ├── error-toast/
│   │   │   │   └── confirm-dialog/
│   │   │   └── pipes/
│   │   │       └── status-nota.pipe.ts
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts              # URLs locais
│   │   └── environment.prod.ts         # URLs Supabase/produção
│   └── styles.scss
├── angular.json
├── package.json
└── tsconfig.json
```

## Projetos .NET na Solution

```
Korp.sln contém:
├── src/backend/Korp.SharedKernel/Korp.SharedKernel.csproj
├── src/backend/Korp.Estoque/Korp.Estoque.Domain/Korp.Estoque.Domain.csproj
├── src/backend/Korp.Estoque/Korp.Estoque.Application/Korp.Estoque.Application.csproj
├── src/backend/Korp.Estoque/Korp.Estoque.Infrastructure/Korp.Estoque.Infrastructure.csproj
├── src/backend/Korp.Estoque/Korp.Estoque.API/Korp.Estoque.API.csproj
├── src/backend/Korp.Faturamento/Korp.Faturamento.Domain/Korp.Faturamento.Domain.csproj
├── src/backend/Korp.Faturamento/Korp.Faturamento.Application/Korp.Faturamento.Application.csproj
├── src/backend/Korp.Faturamento/Korp.Faturamento.Infrastructure/Korp.Faturamento.Infrastructure.csproj
└── src/backend/Korp.Faturamento/Korp.Faturamento.API/Korp.Faturamento.API.csproj
```

## Referências entre projetos (.csproj)

```
Estoque.Domain        → SharedKernel
Estoque.Application   → Domain, SharedKernel
Estoque.Infrastructure→ Application, Domain
Estoque.API           → Application, Infrastructure

Faturamento.Domain    → SharedKernel
Faturamento.Application → Domain, SharedKernel
Faturamento.Infrastructure → Application, Domain
Faturamento.API       → Application, Infrastructure
```
