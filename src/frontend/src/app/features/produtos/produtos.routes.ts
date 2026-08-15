import { Routes } from '@angular/router';

export const produtosRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/lista/produtos-lista.component').then((m) => m.ProdutosListaComponent),
  },
  {
    path: 'novo',
    loadComponent: () =>
      import('./pages/form/produto-form.component').then((m) => m.ProdutoFormComponent),
  },
  {
    path: ':id/editar',
    loadComponent: () =>
      import('./pages/form/produto-form.component').then((m) => m.ProdutoFormComponent),
  },
];
