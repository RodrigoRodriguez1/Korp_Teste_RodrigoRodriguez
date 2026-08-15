import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotaFiscal, CreateNotaFiscalRequest } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.faturamentoApiUrl}/notas-fiscais`;

  getAll(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.baseUrl);
  }

  getById(id: string): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateNotaFiscalRequest): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, request);
  }

  imprimir(id: string, idempotencyKey: string): Observable<NotaFiscal> {
    const headers = new HttpHeaders({ 'Idempotency-Key': idempotencyKey });
    return this.http.post<NotaFiscal>(`${this.baseUrl}/${id}/imprimir`, {}, { headers });
  }
}
