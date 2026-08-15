import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ProdutoService } from '../../../../core/services/produto.service';
import { Produto } from '../../../../core/models/produto.model';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-produtos-lista',
  imports: [
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './produtos-lista.component.html',
  styleUrl: './produtos-lista.component.scss',
})
export class ProdutosListaComponent implements OnInit {
  private readonly service = inject(ProdutoService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly produtos = signal<Produto[]>([]);
  readonly loading = signal(false);
  readonly displayedColumns = ['codigo', 'descricao', 'saldo', 'acoes'];

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (data) => this.produtos.set(data),
      error: () => {},
      complete: () => this.loading.set(false),
    });
  }

  confirmarDelete(produto: Produto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: 'Excluir Produto',
        mensagem: `Deseja excluir o produto "${produto.descricao}" (${produto.codigo})?`,
      },
    });

    ref.afterClosed().subscribe((confirmado: boolean) => {
      if (confirmado) {
        this.service.delete(produto.id).subscribe({
          next: () => {
            this.snackBar.open('Produto excluído.', 'OK', { duration: 3000 });
            this.carregar();
          },
        });
      }
    });
  }
}
