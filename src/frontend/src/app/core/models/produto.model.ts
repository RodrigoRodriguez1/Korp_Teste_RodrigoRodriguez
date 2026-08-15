export interface Produto {
  id: string;
  codigo: string;
  descricao: string;
  saldo: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProdutoRequest {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface UpdateProdutoRequest {
  codigo: string;
  descricao: string;
  saldo: number;
}
