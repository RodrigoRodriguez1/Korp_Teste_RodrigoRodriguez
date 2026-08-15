import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotaFiscalService } from '../../../../core/services/nota-fiscal.service';
import { NotaFiscal } from '../../../../core/models/nota-fiscal.model';

@Component({
  selector: 'app-notas-fiscais-lista',
  imports: [
    RouterLink,
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './notas-fiscais-lista.component.html',
  styleUrl: './notas-fiscais-lista.component.scss',
})
export class NotasFiscaisListaComponent implements OnInit {
  private readonly service = inject(NotaFiscalService);

  readonly notas = signal<NotaFiscal[]>([]);
  readonly loading = signal(false);
  readonly displayedColumns = ['numero', 'status', 'itens', 'createdAt', 'acoes'];

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (data) => this.notas.set(data),
      error: () => {},
      complete: () => this.loading.set(false),
    });
  }
}
