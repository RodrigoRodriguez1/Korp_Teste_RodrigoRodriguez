import { Component, inject, signal, OnInit, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { NotaFiscalService } from '../../../../core/services/nota-fiscal.service';
import { NotaFiscal } from '../../../../core/models/nota-fiscal.model';
import { ImprimirDialogComponent } from '../../components/imprimir-dialog/imprimir-dialog.component';

@Component({
  selector: 'app-notas-fiscais-detalhe',
  imports: [
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ],
  templateUrl: './notas-fiscais-detalhe.component.html',
  styleUrl: './notas-fiscais-detalhe.component.scss',
})
export class NotasFiscaisDetalheComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly service = inject(NotaFiscalService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly nota = signal<NotaFiscal | null>(null);
  readonly loading = signal(false);
  readonly imprimindo = signal(false);

  readonly displayedColumns = ['codigo', 'descricao', 'quantidade'];

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.service.getById(this.id()).subscribe({
      next: (data) => this.nota.set(data),
      error: () => {},
      complete: () => this.loading.set(false),
    });
  }

  abrirDialogImprimir(): void {
    const ref = this.dialog.open(ImprimirDialogComponent, {
      width: '480px',
      data: { notaId: this.id() },
    });

    ref.afterClosed().subscribe((resultado: NotaFiscal | undefined) => {
      if (resultado) {
        this.nota.set(resultado);
        this.snackBar.open(
          `Nota fiscal #${resultado.numero} impressa com sucesso!`,
          'OK',
          { duration: 4000 }
        );
      }
    });
  }
}
