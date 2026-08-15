using FluentValidation;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.ImprimirNotaFiscal;

public sealed class ImprimirNotaFiscalCommandValidator : AbstractValidator<ImprimirNotaFiscalCommand>
{
    public ImprimirNotaFiscalCommandValidator()
    {
        RuleFor(x => x.NotaFiscalId)
            .NotEmpty().WithMessage("O id da nota fiscal é obrigatório.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("O Idempotency-Key é obrigatório.")
            .MaximumLength(128).WithMessage("O Idempotency-Key deve ter no máximo 128 caracteres.");
    }
}
