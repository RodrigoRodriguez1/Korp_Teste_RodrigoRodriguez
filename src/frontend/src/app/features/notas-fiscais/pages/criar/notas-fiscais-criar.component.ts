import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotaFiscalService } from '../../../../core/services/nota-fiscal.service';
import { ProdutoService } from '../../../../core/services/produto.service';
import { Produto } from '../../../../core/models/produto.model';

@Component({
  selector: 'app-notas-fiscais-criar',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './notas-fiscais-criar.component.html',
  styleUrl: './notas-fiscais-criar.component.scss',
})
export class NotasFiscaisCriarComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly service = inject(NotaFiscalService);
  private readonly produtoService = inject(ProdutoService);
  private readonly snackBar = inject(MatSnackBar);

  readonly produtos = signal<Produto[]>([]);
  readonly salvando = signal(false);

  readonly form: FormGroup = this.fb.group({
    itens: this.fb.array([], Validators.required),
  });

  get itens(): FormArray {
    return this.form.get('itens') as FormArray;
  }

  ngOnInit(): void {
    this.produtoService.getAll().subscribe({
      next: (data) => this.produtos.set(data),
    });
    this.adicionarItem();
  }

  adicionarItem(): void {
    this.itens.push(
      this.fb.group({
        produtoId: ['', Validators.required],
        quantidade: [1, [Validators.required, Validators.min(1)]],
      })
    );
  }

  removerItem(index: number): void {
    this.itens.removeAt(index);
  }

  getProduto(id: string): Produto | undefined {
    return this.produtos().find((p) => p.id === id);
  }

  salvar(): void {
    if (this.form.invalid || this.itens.length === 0) {
      this.form.markAllAsTouched();
      return;
    }

    const itens = this.itens.controls.map((ctrl) => {
      const produto = this.getProduto(ctrl.value.produtoId)!;
      return {
        produtoId: produto.id,
        produtoCodigo: produto.codigo,
        produtoDescricao: produto.descricao,
        quantidade: ctrl.value.quantidade,
      };
    });

    this.salvando.set(true);
    this.service.create({ itens }).subscribe({
      next: (nota) => {
        this.snackBar.open('Nota fiscal criada com sucesso!', 'OK', { duration: 3000 });
        this.router.navigate(['/notas-fiscais', nota.id]);
      },
      error: () => this.salvando.set(false),
    });
  }
}
