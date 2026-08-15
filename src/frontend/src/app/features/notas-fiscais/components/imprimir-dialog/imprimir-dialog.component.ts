import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotaFiscalService } from '../../../../core/services/nota-fiscal.service';

interface DialogData {
  notaId: string;
}

@Component({
  selector: 'app-imprimir-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './imprimir-dialog.component.html',
  styleUrl: './imprimir-dialog.component.scss',
})
export class ImprimirDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ImprimirDialogComponent>);
  private readonly data = inject<DialogData>(MAT_DIALOG_DATA);
  private readonly service = inject(NotaFiscalService);

  readonly imprimindo = signal(false);

  readonly form = this.fb.group({
    idempotencyKey: [this.gerarChave(), [Validators.required, Validators.maxLength(128)]],
  });

  private gerarChave(): string {
    return `nf-${this.data.notaId}-${Date.now()}`;
  }

  regenerarChave(): void {
    this.form.patchValue({ idempotencyKey: this.gerarChave() });
  }

  confirmar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.imprimindo.set(true);
    this.service
      .imprimir(this.data.notaId, this.form.value.idempotencyKey!)
      .subscribe({
        next: (nota) => this.dialogRef.close(nota),
        error: () => {
          this.imprimindo.set(false);
        },
      });
  }

  cancelar(): void {
    this.dialogRef.close();
  }
}
