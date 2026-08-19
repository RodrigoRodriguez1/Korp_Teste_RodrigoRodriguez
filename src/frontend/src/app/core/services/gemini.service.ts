import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GeminiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl =
    'https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent';

  sugerirCodigo(descricao: string): Observable<string | null> {
    if (!environment.geminiApiKey || !descricao.trim()) return of(null);

    const prompt = `Gere um código de produto curto, padronizado e em maiúsculas para o seguinte produto: "${descricao}".
O código deve ter no máximo 15 caracteres, usar apenas letras, números e hífens, sem espaços.
Exemplos: "NTB-DELL-15", "CAD-ESC-PRE", "MNT-LG-27".
Responda APENAS com o código, sem explicações.`;

    const body = {
      contents: [{ parts: [{ text: prompt }] }],
      generationConfig: { maxOutputTokens: 500, temperature: 0.3 },
    };

    return this.http
      .post<any>(`${this.apiUrl}?key=${environment.geminiApiKey}`, body)
      .pipe(
        map((res) => {
          const texto: string =
            res?.candidates?.[0]?.content?.parts?.[0]?.text ?? '';
          return texto.trim().toUpperCase().slice(0, 20) || null;
        }),
        catchError(() => of(null))
      );
  }
}
