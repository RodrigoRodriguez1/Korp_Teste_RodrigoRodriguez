# 07 — Arquitetura Frontend (Angular)

## Versão e configuração

- Angular 21 (CLI instalado: 21.2.13)
- Standalone Components — sem NgModules
- Strict TypeScript
- Base href: `/korp/` (para funcionar via redirect do Vercel)

## Rotas

```
/korp/                          → redirect para /korp/produtos
/korp/produtos                  → ProdutosListComponent
/korp/produtos/novo             → ProdutoFormComponent (criar)
/korp/produtos/:id/editar       → ProdutoFormComponent (editar)
/korp/notas-fiscais             → NotasListComponent
/korp/notas-fiscais/nova        → NotaFormComponent
/korp/notas-fiscais/:id         → NotaDetailComponent
```

## Fluxo de telas

### Tela 1 — Lista de Produtos (`/korp/produtos`)
- Tabela com colunas: Código | Descrição | Saldo | Ações
- Ações: Editar | Excluir
- Botão "Novo Produto" → navega para `/korp/produtos/novo`
- Loading skeleton enquanto carrega
- Estado vazio: mensagem "Nenhum produto cadastrado"

### Tela 2 — Formulário de Produto (`/korp/produtos/novo` e `/editar`)
- Reactive Form com campos: Código, Descrição, Saldo
- Validação em tempo real com mensagens de erro abaixo de cada campo
- Botão "Salvar" (desabilitado se form inválido)
- Botão "Cancelar" → volta para lista
- Em edição: pre-preenche os campos com dados do produto

### Tela 3 — Lista de Notas Fiscais (`/korp/notas-fiscais`)
- Tabela com colunas: Número | Status (badge colorido) | Qtd Itens | Data | Ações
- Status badge: verde para Aberta, cinza para Fechada
- Ações: Ver detalhes | Imprimir (apenas para Aberta)
- Botão "Nova Nota Fiscal"

### Tela 4 — Formulário de Nota Fiscal (`/korp/notas-fiscais/nova`)
- Seção "Adicionar Produto":
  - Select/Autocomplete de produtos (carregado do Estoque.API)
  - Campo de quantidade
  - Botão "Adicionar item"
- Tabela de itens adicionados: Código | Descrição | Quantidade | Remover
- Botão "Criar Nota Fiscal" (mínimo 1 item)
- Feedback: saldo disponível exibido ao selecionar produto

### Tela 5 — Detalhe da Nota Fiscal (`/korp/notas-fiscais/:id`)
- Cabeçalho: Número, Status (badge), Data de criação
- Tabela de itens: Código | Descrição | Quantidade
- Botão "Imprimir Nota" (visível e habilitado apenas se status = Aberta)
  - Ao clicar: spinner no botão, botão desabilitado
  - Sucesso: status atualiza para Fechada, toast de sucesso
  - Erro: toast com mensagem do backend, botão volta ao normal

## Gerenciamento de estado

### Signals (estado local de componente)
```typescript
// Loading states
loading = signal(false);
loadingImprimir = signal(false);

// Dados
produtos = signal<Produto[]>([]);
notasFiscais = signal<NotaFiscal[]>([]);
notaAtual = signal<NotaFiscal | null>(null);

// UI
itensDaNota = signal<ItemNotaForm[]>([]);
```

### RxJS (streams HTTP)
```typescript
// Serviços retornam Observable
getProdutos(): Observable<Produto[]>
createProduto(dto: CreateProdutoRequest): Observable<Produto>
imprimirNota(id: string): Observable<NotaFiscal>

// Nos componentes: takeUntilDestroyed() para cleanup automático
this.produtoService.getProdutos()
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe(produtos => this.produtos.set(produtos));
```

## Ciclos de vida Angular utilizados

| Hook | Componente | Uso |
|------|-----------|-----|
| `ngOnInit` | ProdutosListComponent, NotasListComponent, NotaDetailComponent, ProdutoFormComponent (em edição) | Carrega dados da API ao inicializar |
| `ngOnDestroy` | Componentes com subscriptions manuais | Cleanup via `takeUntilDestroyed()` (automático com DestroyRef) |
| `ngAfterViewInit` | NotaFormComponent | Foco no primeiro campo do formulário após renderização |

## Bibliotecas Angular utilizadas

| Biblioteca | Finalidade |
|-----------|-----------|
| `@angular/forms` (ReactiveFormsModule) | Formulários com validação |
| `@angular/common/http` (HttpClient) | Chamadas às APIs REST |
| `@angular/router` | Navegação entre telas |
| `rxjs` | Operadores em streams HTTP (map, catchError, finalize, takeUntilDestroyed) |
| Angular Material (`@angular/material`) | Componentes visuais: tabela, botões, inputs, select, spinner, snackbar, dialog |

> **Escolha do Angular Material:** pedido explicitamente pelo desafio identificar "bibliotecas para componentes visuais". Material é a biblioteca oficial Angular, mantida pelo Google, com suporte nativo a acessibilidade e temas.

## Interceptors

### ErrorInterceptor
```typescript
// Captura erros HTTP globalmente e exibe toast via MatSnackBar
// Mapeamento de status → mensagem amigável:
// 400 → "Dados inválidos: <detalhe do backend>"
// 404 → "Recurso não encontrado."
// 409 → "<detalhe do backend>"  (regras de negócio)
// 503 → "Serviço de estoque indisponível. Tente novamente."
// 500 → "Erro interno. Tente novamente mais tarde."
```

### LoadingInterceptor
```typescript
// Incrementa/decrementa contador de requests ativos
// Exibe/esconde spinner global no toolbar
```

## Environments

```typescript
// environment.ts (local)
export const environment = {
  production: false,
  estoqueApiUrl: 'http://localhost:5002',
  faturamentoApiUrl: 'http://localhost:5001',
};

// environment.prod.ts
export const environment = {
  production: true,
  estoqueApiUrl: 'https://<supabase-ou-render>/estoque',
  faturamentoApiUrl: 'https://<supabase-ou-render>/faturamento',
};
```

## Build para produção

```bash
ng build --base-href /korp/ --configuration production
```

O output em `dist/` é enviado ao Cloudflare Pages. O `vercel.json` do portfólio redireciona `/korp/(.*)` para `https://korp-teste-rodrigo.pages.dev/korp/$1`.
