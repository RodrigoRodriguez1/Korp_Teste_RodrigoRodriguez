# 10 — Critérios de Aceite

## Como usar este documento

Cada etapa de desenvolvimento termina com uma checklist de validação. Só avançamos para a próxima etapa quando todos os itens estão marcados.

Fluxo: **Spec aprovada → Implementação → Testes → Validação (checklist abaixo) → Próxima etapa**

---

## Etapa 1 — Setup do Projeto

**Critérios:**
- [ ] Repositório `Korp_Teste_RodrigoRodriguez` criado no GitHub como público
- [ ] Solution `.sln` criada com todos os projetos referenciados
- [ ] `docker-compose.yml` funciona: `docker compose up` sobe PostgreSQL na porta 5432
- [ ] Script `init.sql` cria schemas `estoque` e `faturamento` + sequence
- [ ] `dotnet build` na solution passa sem erros
- [ ] Projeto Angular criado com `ng new` e configurado com base-href `/korp/`
- [ ] `ng build` passa sem erros
- [ ] `.gitignore` correto (sem `bin/`, `obj/`, `node_modules/`, `appsettings.Development.json`)
- [ ] README.md com instruções de como rodar localmente

---

## Etapa 2 — Microsserviço Estoque (backend)

**Critérios:**
- [ ] `POST /produtos` cria produto e retorna 201 com o produto criado
- [ ] `GET /produtos` retorna lista de produtos (vazia inicialmente)
- [ ] `GET /produtos/{id}` retorna produto ou 404
- [ ] `PUT /produtos/{id}` atualiza produto
- [ ] `DELETE /produtos/{id}` remove produto
- [ ] Código duplicado retorna 409
- [ ] Saldo negativo retorna 400 com detalhe do campo
- [ ] `POST /produtos/descontar-saldo` desconta saldo atomicamente
- [ ] `POST /produtos/descontar-saldo` com saldo insuficiente retorna 409 e não altera nenhum saldo
- [ ] Erros retornam ProblemDetails (RFC 7807)
- [ ] Migrations do EF Core aplicadas com sucesso (`dotnet ef database update`)
- [ ] Dados persistidos no PostgreSQL (verificável via `psql` ou DBeaver)
- [ ] Testes unitários do domínio passando (`dotnet test`)

---

## Etapa 3 — Microsserviço Faturamento (backend)

**Critérios:**
- [ ] `POST /notas-fiscais` cria NF com status `Aberta` e número sequencial
- [ ] Números de NF são sequenciais (1, 2, 3...) e nunca se repetem
- [ ] `GET /notas-fiscais` retorna lista com itens
- [ ] `GET /notas-fiscais/{id}` retorna NF com itens ou 404
- [ ] `POST /notas-fiscais/{id}/imprimir` com NF Aberta:
  - [ ] Chama Estoque e desconta saldo
  - [ ] Atualiza status para `Fechada`
  - [ ] Preenche `impressoEm`
  - [ ] Retorna NF atualizada
- [ ] `POST /notas-fiscais/{id}/imprimir` com NF Fechada retorna 409
- [ ] `POST /notas-fiscais/{id}/imprimir` com Estoque offline retorna 503 e NF permanece Aberta
- [ ] Polly configurado (retry + circuit breaker visível nos logs)
- [ ] Testes unitários dos handlers passando

---

## Etapa 4 — Frontend Angular

**Critérios:**
- [ ] Navegação entre telas funciona sem reload de página
- [ ] Tela de Produtos: lista carrega produtos da API
- [ ] Tela de Produtos: criar produto funciona (formulário + feedback)
- [ ] Tela de Produtos: editar produto funciona
- [ ] Tela de Produtos: excluir produto funciona (com confirmação)
- [ ] Tela de NF: lista carrega notas com status correto (badge colorido)
- [ ] Tela de Nova NF: select de produtos carrega do Estoque.API
- [ ] Tela de Nova NF: saldo disponível exibido ao selecionar produto
- [ ] Tela de Nova NF: criar com itens funciona
- [ ] Tela Detalhe NF: botão "Imprimir" visível apenas para NF Aberta
- [ ] Botão Imprimir: exibe spinner durante processamento
- [ ] Botão Imprimir: sucesso atualiza status na tela para Fechada
- [ ] Botão Imprimir: erro de saldo insuficiente exibe mensagem amigável
- [ ] Botão Imprimir: Estoque offline exibe mensagem amigável
- [ ] Sem erros de TypeScript (`ng build --strict`)
- [ ] RxJS usado nos serviços HTTP com `catchError` e `finalize`
- [ ] Angular Material usado para componentes visuais

---

## Etapa 5 — Integração e Deploy

**Critérios:**
- [ ] Frontend buildado com `--base-href /korp/` funciona sem erros de assets
- [ ] Cloudflare Pages serve o Angular em `korp-teste-rodrigo.pages.dev/korp/`
- [ ] Render.com serve Estoque.API e Faturamento.API (pode ter cold start ~30s)
- [ ] Supabase tem os schemas e dados persistidos
- [ ] `vercel.json` do portfólio atualizado com rewrite para `/korp/*`
- [ ] URL `portfolio-rodrigo-rodriguez.vercel.app/korp` carrega o sistema
- [ ] Fluxo completo funciona em produção: criar produto → criar NF → imprimir NF

---

## Etapa 6 — Opcionais e Requisitos de Entrega

**Critérios:**
- [ ] **Concorrência:** demonstrável (2 requests simultâneos, 1 sucede, 1 falha com 409)
- [ ] **Idempotência:** reenviar mesma `Idempotency-Key` não desconta saldo 2x
- [ ] **Simulação de falha:** parar Estoque.API → Angular exibe mensagem adequada → subir novamente → funciona
- [ ] README.md explica como rodar, como fazer deploy e como simular falha
- [ ] Documento de detalhamento técnico criado (responde todas as perguntas do desafio)
- [ ] Vídeo gravado demonstrando telas, funcionalidades e explicação técnica
- [ ] Repositório público e acessível

---

## Perguntas do desafio que precisam ser respondidas no vídeo/documento

1. Quais **ciclos de vida do Angular** foram utilizados → `ngOnInit`, `ngOnDestroy` (via `takeUntilDestroyed`), `ngAfterViewInit`
2. **RxJS** usado? Como? → Sim: `HttpClient` retorna `Observable`; operadores `map`, `catchError`, `finalize`, `takeUntilDestroyed`
3. Outras **bibliotecas** utilizadas → Angular Material (componentes visuais), FluentValidation, MediatR, Polly, EF Core
4. **Componentes visuais** → Angular Material (`mat-table`, `mat-button`, `mat-form-field`, `mat-select`, `mat-spinner`, `mat-snack-bar`, `mat-badge`)
5. **Gerenciamento de dependências Go** → Não aplicável (usamos C#)
6. **Frameworks C#** → ASP.NET Core 9 Minimal API, Entity Framework Core 9, MediatR, FluentValidation, Polly
7. **Erros e exceções no backend** → Result Pattern para negócio, ExceptionMiddleware global para inesperados, ProblemDetails (RFC 7807)
8. **LINQ** → Sim: projeções com `.Select()` nas queries de leitura, filtros com `.Where()`, `ToListAsync()` com `CancellationToken`
