# 06 — Regras de Negócio

## Produtos

| # | Regra | Onde validar |
|---|-------|-------------|
| P1 | `codigo` é obrigatório, máximo 20 caracteres | FluentValidation |
| P2 | `descricao` é obrigatória, máximo 200 caracteres | FluentValidation |
| P3 | `saldo` não pode ser negativo | FluentValidation |
| P4 | `codigo` deve ser único por produto | Repository (retorna Conflict) |
| P5 | Não é possível excluir produto que está em uma NF com status Aberta | Handler (retorna Conflict) |
| P6 | O saldo nunca pode ficar negativo após desconto | Entidade `Produto.DescontarSaldo()` |

## Notas Fiscais

| # | Regra | Onde validar |
|---|-------|-------------|
| N1 | Toda NF nasce com status `Aberta` | Handler `CreateNotaFiscal` |
| N2 | O número da NF é gerado automaticamente via SEQUENCE do PostgreSQL | Banco de dados |
| N3 | Uma NF deve ter pelo menos 1 item | FluentValidation |
| N4 | A quantidade de cada item deve ser maior que zero | FluentValidation |
| N5 | Não é possível adicionar itens a uma NF `Fechada` | Entidade `NotaFiscal.AdicionarItem()` |
| N6 | Não é possível imprimir uma NF com status `Fechada` | Handler `ImprimirNotaFiscal` |
| N7 | Não é possível imprimir uma NF se qualquer produto tiver saldo insuficiente | Estoque.API (retorna 409) |

## Fluxo de impressão (regra crítica)

```
1. Angular envia POST /notas-fiscais/{id}/imprimir
2. Faturamento verifica: NF existe? → 404 se não
3. Faturamento verifica: NF está Aberta? → 409 se Fechada
4. Faturamento chama Estoque: POST /produtos/descontar-saldo com todos os itens
   4a. Estoque verifica saldo de CADA produto (dentro de transação)
   4b. Se qualquer produto tiver saldo insuficiente → 409, nenhum saldo é descontado
   4c. Se Estoque indisponível → 503
5. Se Estoque respondeu 200:
   5a. Faturamento atualiza status da NF para Fechada
   5b. Faturamento preenche ImpressoEm com timestamp atual
   5c. Faturamento retorna NF atualizada com status 200
6. Se Estoque respondeu erro:
   6a. NF permanece com status Aberta
   6b. Faturamento repassa o erro ao Angular com ProblemDetails
```

## Atomicidade do desconto de saldo

O endpoint `POST /produtos/descontar-saldo` no Estoque.API executa todo o desconto dentro de **uma única transação de banco de dados**:

```
BEGIN TRANSACTION
  SELECT * FROM produtos WHERE id IN (...) FOR UPDATE  ← lock nas linhas
  verificar saldo de cada produto
  se qualquer saldo insuficiente → ROLLBACK → retorna 409
  UPDATE saldo de todos os produtos
COMMIT
```

Isso garante que:
- Se dois requests tentam usar o mesmo produto simultaneamente, um aguarda o lock
- Nunca haverá saldo negativo
- Ou todos os saldos são descontados ou nenhum é

## Regras de idempotência (opcional implementado)

- O Angular gera um UUID v4 antes de enviar o request de impressão
- Envia no header `Idempotency-Key: <uuid>`
- Se o Faturamento receber a mesma chave novamente (até 24h), retorna a resposta original sem reprocessar
- Protege contra double-click, retry automático em falha de rede, etc.

## Simulação de falha (requisito obrigatório)

Para demonstrar o tratamento de falhas no vídeo:

1. Parar o Estoque.API manualmente (ou via endpoint `/internal/simular-falha`)
2. Tentar imprimir uma NF no Angular
3. O Angular exibe mensagem: *"O serviço de estoque está temporariamente indisponível. Sua nota fiscal não foi alterada. Tente novamente em alguns segundos."*
4. A NF permanece com status `Aberta`
5. Subir o Estoque.API novamente
6. Tentar imprimir — funciona normalmente

O Faturamento.API usa **Polly** com as seguintes políticas ao chamar o Estoque:
- **Retry:** 3 tentativas com backoff exponencial (1s, 2s, 4s)
- **Circuit Breaker:** após 5 falhas em 30s, abre o circuito por 30s (retorna 503 imediatamente sem chamar Estoque)
- **Timeout:** cada tentativa tem timeout de 5s
