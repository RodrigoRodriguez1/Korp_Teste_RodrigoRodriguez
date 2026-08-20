import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

interface SugerirCodigoResponse {
  codigo: string | null;
}

@Injectable({ providedIn: 'root' })
export class GeminiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.estoqueApiUrl}/produtos/ia/sugerir-codigo`;

  sugerirCodigo(descricao: string): Observable<string | null> {
    if (!descricao.trim() || descricao.trim().length < 4) return of(null);

    return this.http
      .post<SugerirCodigoResponse>(this.baseUrl, { descricao })
      .pipe(
        map((res) => res.codigo || null),
        catchError(() => of(null))
      );
  }
}
