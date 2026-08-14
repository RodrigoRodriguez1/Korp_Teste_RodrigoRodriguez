# 05 — Contratos de API

## Estoque.API — Base URL: `http://localhost:5002`

### Produtos

#### `GET /produtos`
Lista todos os produtos.

**Response 200:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "codigo": "PROD-001",
    "descricao": "Notebook Dell Inspiron",
    "saldo": 10
  }
]
```

---

#### `GET /produtos/{id}`
Busca produto por ID.

**Response 200:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "codigo": "PROD-001",
  "descricao": "Notebook Dell Inspiron",
  "saldo": 10
}
```

**Response 404:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Produto com id '3fa85f64' não foi encontrado."
}
```

---

#### `POST /produtos`
Cria novo produto.

**Request:**
```json
{
  "codigo": "PROD-001",
  "descricao": "Notebook Dell Inspiron",
  "saldo": 10
}
```

**Response 201:** (produto criado, header `Location: /produtos/{id}`)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "codigo": "PROD-001",
  "descricao": "Notebook Dell Inspiron",
  "saldo": 10
}
```

**Response 400:** (validação)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Erro de validação",
  "status": 400,
  "errors": {
    "codigo": ["O código é obrigatório."],
    "saldo": ["O saldo não pode ser negativo."]
  }
}
```

**Response 409:** (código duplicado)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Conflito",
  "status": 409,
  "detail": "Já existe um produto com o código 'PROD-001'."
}
```

---

#### `PUT /produtos/{id}`
Atualiza produto.

**Request:**
```json
{
  "codigo": "PROD-001",
  "descricao": "Notebook Dell Inspiron 15",
  "saldo": 15
}
```

**Response 200:** (produto atualizado)
**Response 404:** produto não encontrado
**Response 409:** código duplicado em outro produto

---

#### `DELETE /produtos/{id}`
Remove produto.

**Response 204:** removido com sucesso
**Response 404:** produto não encontrado
**Response 409:** produto está em uso em alguma NF Aberta

---

#### `POST /produtos/descontar-saldo` *(endpoint interno — chamado pelo Faturamento)*
Desconta saldo de múltiplos produtos atomicamente.

**Request:**
```json
{
  "itens": [
    { "produtoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "quantidade": 2 },
    { "produtoId": "8ab12c34-1234-5678-abcd-ef0123456789", "quantidade": 1 }
  ]
}
```

**Response 200:** saldo descontado com sucesso
```json
{ "sucesso": true }
```

**Response 409:** saldo insuficiente em algum produto
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Saldo insuficiente",
  "status": 409,
  "detail": "Produto 'PROD-001' possui saldo 1, mas a nota requer 2 unidades."
}
```

---

## Faturamento.API — Base URL: `http://localhost:5001`

### Notas Fiscais

#### `GET /notas-fiscais`
Lista todas as notas fiscais com seus itens.

**Response 200:**
```json
[
  {
    "id": "a1b2c3d4-0000-0000-0000-000000000001",
    "numero": 1,
    "status": "Aberta",
    "impressoEm": null,
    "itens": [
      {
        "id": "e5f6a7b8-0000-0000-0000-000000000001",
        "produtoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "produtoCodigo": "PROD-001",
        "produtoDescricao": "Notebook Dell Inspiron",
        "quantidade": 2
      }
    ],
    "criadoEm": "2026-08-14T10:00:00Z"
  }
]
```

---

#### `GET /notas-fiscais/{id}`
Busca NF por ID.

**Response 200:** (mesmo formato do item acima)
**Response 404:** NF não encontrada

---

#### `POST /notas-fiscais`
Cria nova nota fiscal. Número gerado automaticamente pela sequence.

**Request:**
```json
{
  "itens": [
    {
      "produtoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "produtoCodigo": "PROD-001",
      "produtoDescricao": "Notebook Dell Inspiron",
      "quantidade": 2
    }
  ]
}
```

> Os campos `produtoCodigo` e `produtoDescricao` são enviados pelo Angular (já buscou os produtos do Estoque) e armazenados como snapshot na NF.

**Response 201:**
```json
{
  "id": "a1b2c3d4-0000-0000-0000-000000000001",
  "numero": 42,
  "status": "Aberta",
  "impressoEm": null,
  "itens": [...],
  "criadoEm": "2026-08-14T10:00:00Z"
}
```

**Response 400:** lista de itens vazia ou quantidade inválida

---

#### `POST /notas-fiscais/{id}/imprimir`
Imprime a NF: fecha a nota e desconta saldo dos produtos.

**Headers opcionais (idempotência):**
```
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
```

**Response 200:**
```json
{
  "id": "a1b2c3d4-0000-0000-0000-000000000001",
  "numero": 42,
  "status": "Fechada",
  "impressoEm": "2026-08-14T10:05:00Z",
  "itens": [...],
  "criadoEm": "2026-08-14T10:00:00Z"
}
```

**Response 404:** NF não encontrada

**Response 409 — NF já fechada:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Operação inválida",
  "status": 409,
  "detail": "A nota fiscal 42 já está fechada e não pode ser impressa novamente."
}
```

**Response 409 — Saldo insuficiente:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Saldo insuficiente",
  "status": 409,
  "detail": "Produto 'PROD-001' possui saldo 1, mas a nota requer 2 unidades."
}
```

**Response 503 — Estoque indisponível:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Serviço indisponível",
  "status": 503,
  "detail": "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns segundos."
}
```

---

## Códigos HTTP utilizados

| Código | Significado | Quando usar |
|--------|-------------|-------------|
| 200 | OK | GET com resultado, POST de ação (imprimir) |
| 201 | Created | POST de criação de recurso |
| 204 | No Content | DELETE bem-sucedido |
| 400 | Bad Request | Falha de validação FluentValidation |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Regra de negócio violada (saldo, status, duplicidade) |
| 503 | Service Unavailable | Microsserviço dependente indisponível |

## CORS

Faturamento.API e Estoque.API aceitam requests de:
- `http://localhost:4200` (Angular dev)
- `https://korp-teste-rodrigo.pages.dev` (Cloudflare Pages produção)
- `https://portfolio-rodrigo-rodriguez.vercel.app` (portfólio)
