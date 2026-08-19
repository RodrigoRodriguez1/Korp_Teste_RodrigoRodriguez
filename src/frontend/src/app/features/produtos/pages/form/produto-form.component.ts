import { Component, inject, signal, OnInit, input, DestroyRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProdutoService } from '../../../../core/services/produto.service';
import { GeminiService } from '../../../../core/services/gemini.service';

@Component({
  selector: 'app-produto-form',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './produto-form.component.html',
  styleUrl: './produto-form.component.scss',
})
export class ProdutoFormComponent implements OnInit {
  readonly id = input<string>();

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly service = inject(ProdutoService);
  private readonly gemini = inject(GeminiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly salvando = signal(false);
  readonly carregando = signal(false);
  readonly sugestaoCodigo = signal<string | null>(null);
  readonly buscandoSugestao = signal(false);

  readonly form = this.fb.group({
    codigo: ['', [Validators.required, Validators.maxLength(20)]],
    descricao: ['', [Validators.required, Validators.maxLength(200)]],
    saldo: [0, [Validators.required, Validators.min(0)]],
  });

  get editando(): boolean {
    return !!this.id();
  }

  ngOnInit(): void {
    if (this.editando) {
      this.carregando.set(true);
      this.service.getById(this.id()!).subscribe({
        next: (p) => {
          this.form.patchValue({ codigo: p.codigo, descricao: p.descricao, saldo: p.saldo });
        },
        error: () => this.router.navigate(['/produtos']),
        complete: () => this.carregando.set(false),
      });
    } else {
      this.configurarSugestaoIA();
    }
  }

  private configurarSugestaoIA(): void {
    this.form.get('descricao')!.valueChanges.pipe(
      debounceTime(600),
      distinctUntilChanged(),
      switchMap((descricao) => {
        if (!descricao || descricao.trim().length < 4) {
          this.sugestaoCodigo.set(null);
          return [];
        }
        this.buscandoSugestao.set(true);
        return this.gemini.sugerirCodigo(descricao);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (sugestao) => {
        this.sugestaoCodigo.set(sugestao);
        this.buscandoSugestao.set(false);
      },
      error: () => this.buscandoSugestao.set(false),
    });
  }

  usarSugestao(): void {
    const sugestao = this.sugestaoCodigo();
    if (sugestao) {
      this.form.get('codigo')!.setValue(sugestao);
      this.sugestaoCodigo.set(null);
    }
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.value as { codigo: string; descricao: string; saldo: number };
    this.salvando.set(true);

    const request$ = this.editando
      ? this.service.update(this.id()!, payload)
      : this.service.create(payload);

    request$.subscribe({
      next: () => {
        const msg = this.editando ? 'Produto atualizado!' : 'Produto criado!';
        this.snackBar.open(msg, 'OK', { duration: 3000 });
        this.router.navigate(['/produtos']);
      },
      error: () => this.salvando.set(false),
    });
  }
}
