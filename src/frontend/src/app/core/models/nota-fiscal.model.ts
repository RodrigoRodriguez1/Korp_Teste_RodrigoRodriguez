export interface ItemNota {
  id: string;
  produtoId: string;
  produtoCodigo: string;
  produtoDescricao: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numero: number;
  status: 'Aberta' | 'Fechada';
  impressoEm: string | null;
  itens: ItemNota[];
  createdAt: string;
}

export interface CreateItemNotaRequest {
  produtoId: string;
  produtoCodigo: string;
  produtoDescricao: string;
  quantidade: number;
}

export interface CreateNotaFiscalRequest {
  itens: CreateItemNotaRequest[];
}
