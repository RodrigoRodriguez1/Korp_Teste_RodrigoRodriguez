# 04 — Modelo de Dados

## Schema: `estoque`

### Tabela: `produtos`

| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| `id` | `uuid` | PK, default `gen_random_uuid()` | Identificador único |
| `codigo` | `varchar(20)` | NOT NULL, UNIQUE | Código do produto (definido pelo usuário) |
| `descricao` | `varchar(200)` | NOT NULL | Nome/descrição do produto |
| `saldo` | `integer` | NOT NULL, CHECK >= 0 | Quantidade disponível em estoque |
| `created_at` | `timestamptz` | NOT NULL, default `now()` | Data de criação |
| `updated_at` | `timestamptz` | NOT NULL, default `now()` | Última atualização |

**Índices:**
- `idx_produtos_codigo` — UNIQUE em `codigo`

**Entidade de domínio:**
```csharp
public sealed class Produto
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; }
    public string Descricao { get; private set; }
    public int Saldo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Produto() { } // EF Core

    public static Produto Create(string codigo, string descricao, int saldo)
        => new() { Id = Guid.NewGuid(), Codigo = codigo, Descricao = descricao, Saldo = saldo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

    public Result DescontarSaldo(int quantidade)
    {
        if (quantidade > Saldo)
            return Result.Failure(ProdutoErrors.SaldoInsuficiente(Codigo, Saldo, quantidade));
        Saldo -= quantidade;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
```

---

## Schema: `faturamento`

### Sequence: `nota_fiscal_numero_seq`

```sql
CREATE SEQUENCE faturamento.nota_fiscal_numero_seq START 1 INCREMENT 1;
```

Garante numeração sequencial thread-safe — sem race condition de MAX+1.

---

### Tabela: `notas_fiscais`

| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| `id` | `uuid` | PK, default `gen_random_uuid()` | Identificador único |
| `numero` | `integer` | NOT NULL, UNIQUE, default `nextval(seq)` | Numeração sequencial automática |
| `status` | `smallint` | NOT NULL, default 1 | 1 = Aberta, 2 = Fechada |
| `impresso_em` | `timestamptz` | NULL | Preenchido ao imprimir |
| `created_at` | `timestamptz` | NOT NULL, default `now()` | Data de criação |
| `updated_at` | `timestamptz` | NOT NULL, default `now()` | Última atualização |

**Índices:**
- `idx_notas_fiscais_numero` — UNIQUE em `numero`
- `idx_notas_fiscais_status` — em `status` (filtros de listagem)

---

### Tabela: `itens_nota`

| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| `id` | `uuid` | PK, default `gen_random_uuid()` | Identificador único |
| `nota_fiscal_id` | `uuid` | NOT NULL, FK → `notas_fiscais.id` ON DELETE CASCADE | Nota pai |
| `produto_id` | `uuid` | NOT NULL | ID do produto (referência ao Estoque — sem FK cross-schema) |
| `produto_codigo` | `varchar(20)` | NOT NULL | Snapshot do código no momento da NF |
| `produto_descricao` | `varchar(200)` | NOT NULL | Snapshot da descrição no momento da NF |
| `quantidade` | `integer` | NOT NULL, CHECK > 0 | Quantidade utilizada na nota |

> **Nota:** `produto_id`, `produto_codigo` e `produto_descricao` são snapshots do produto no momento da criação da NF. Isso evita JOIN cross-schema e garante que a NF não seja afetada por mudanças futuras no produto.

**Índices:**
- `idx_itens_nota_nota_fiscal_id` — em `nota_fiscal_id`

---

### Tabela: `idempotency_keys` (opcional — idempotência)

| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| `key` | `varchar(36)` | PK | UUID enviado pelo cliente |
| `response_status` | `integer` | NOT NULL | HTTP status da resposta original |
| `response_body` | `jsonb` | NULL | Body da resposta original serializado |
| `created_at` | `timestamptz` | NOT NULL, default `now()` | Data de criação |
| `expires_at` | `timestamptz` | NOT NULL | Data de expiração (24h após criação) |

---

## Entidades de domínio — Faturamento

```csharp
public sealed class NotaFiscal
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public StatusNota Status { get; private set; }
    public DateTime? ImpressoEm { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ItemNota> _itens = [];
    public IReadOnlyCollection<ItemNota> Itens => _itens.AsReadOnly();

    private NotaFiscal() { }

    public static NotaFiscal Create()
        => new() { Id = Guid.NewGuid(), Status = StatusNota.Aberta, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

    public Result AdicionarItem(Guid produtoId, string codigo, string descricao, int quantidade)
    {
        if (Status == StatusNota.Fechada)
            return Result.Failure(NotaFiscalErrors.NotaFechada(Numero));
        _itens.Add(ItemNota.Create(Id, produtoId, codigo, descricao, quantidade));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Fechar()
    {
        if (Status == StatusNota.Fechada)
            return Result.Failure(NotaFiscalErrors.JaFechada(Numero));
        Status = StatusNota.Fechada;
        ImpressoEm = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}

public enum StatusNota { Aberta = 1, Fechada = 2 }
```

---

## Relacionamentos

```
notas_fiscais (1) ──────< itens_nota (N)
     id                     nota_fiscal_id (FK)
```

`itens_nota.produto_id` referencia `estoque.produtos.id` logicamente, mas **não há FK física** (schemas diferentes, microsserviços distintos). A consistência é garantida pela aplicação na criação da NF (busca o produto no Estoque e copia os dados).

---

## Diagrama simplificado

```
[estoque]                    [faturamento]
┌────────────┐               ┌─────────────────┐
│ produtos   │               │ notas_fiscais   │
│────────────│               │─────────────────│
│ id (PK)   │               │ id (PK)         │
│ codigo    │               │ numero (SEQ)    │
│ descricao │               │ status          │
│ saldo     │               │ impresso_em     │
└────────────┘               └────────┬────────┘
                                      │ 1:N
                             ┌────────▼────────┐
                             │ itens_nota      │
                             │─────────────────│
                             │ id (PK)         │
                             │ nota_fiscal_id  │
                             │ produto_id      │ ← referência lógica
                             │ produto_codigo  │ ← snapshot
                             │ produto_descr.  │ ← snapshot
                             │ quantidade      │
                             └─────────────────┘
```
