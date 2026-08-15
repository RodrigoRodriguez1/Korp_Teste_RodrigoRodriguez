using FluentValidation;

namespace Korp.Estoque.Application.Products.Commands.CreateProduto;

public sealed class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
    public CreateProdutoCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O código é obrigatório.")
            .MaximumLength(20).WithMessage("O código deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(200).WithMessage("A descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Saldo)
            .GreaterThanOrEqualTo(0).WithMessage("O saldo não pode ser negativo.");
    }
}
