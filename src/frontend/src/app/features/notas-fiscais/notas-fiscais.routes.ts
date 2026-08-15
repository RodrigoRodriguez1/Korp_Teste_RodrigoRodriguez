import { Routes } from '@angular/router';

export const notasFiscaisRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/lista/notas-fiscais-lista.component').then(
        (m) => m.NotasFiscaisListaComponent
      ),
  },
  {
    path: 'nova',
    loadComponent: () =>
      import('./pages/criar/notas-fiscais-criar.component').then(
        (m) => m.NotasFiscaisCriarComponent
      ),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/detalhe/notas-fiscais-detalhe.component').then(
        (m) => m.NotasFiscaisDetalheComponent
      ),
  },
];
