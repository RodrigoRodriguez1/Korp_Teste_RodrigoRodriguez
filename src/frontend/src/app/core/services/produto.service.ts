import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Produto, CreateProdutoRequest, UpdateProdutoRequest } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.estoqueApiUrl}/produtos`;

  getAll(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl);
  }

  getById(id: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, request);
  }

  update(id: string, request: UpdateProdutoRequest): Observable<Produto> {
    return this.http.put<Produto>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
