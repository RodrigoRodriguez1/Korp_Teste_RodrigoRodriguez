import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'notas-fiscais',
    pathMatch: 'full',
  },
  {
    path: 'notas-fiscais',
    loadChildren: () =>
      import('./features/notas-fiscais/notas-fiscais.routes').then((m) => m.notasFiscaisRoutes),
  },
  {
    path: 'produtos',
    loadChildren: () =>
      import('./features/produtos/produtos.routes').then((m) => m.produtosRoutes),
  },
];
