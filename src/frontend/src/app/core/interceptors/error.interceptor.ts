import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err) => {
      const detail: string =
        err?.error?.detail ??
        err?.error?.title ??
        'Ocorreu um erro inesperado.';

      const status: number = err?.status ?? 0;

      if (status === 503) {
        snackBar.open(`Serviço indisponível: ${detail}`, 'Fechar', {
          duration: 8000,
          panelClass: ['snack-error'],
        });
      } else if (status >= 400 && status < 500) {
        snackBar.open(detail, 'Fechar', { duration: 5000, panelClass: ['snack-warn'] });
      } else if (status >= 500) {
        snackBar.open('Erro interno do servidor. Tente novamente.', 'Fechar', {
          duration: 5000,
          panelClass: ['snack-error'],
        });
      }

      return throwError(() => err);
    })
  );
};
