using FluentValidation;

namespace Korp.Estoque.Application.Products.Commands.DescontarSaldo;

public sealed class DescontarSaldoCommandValidator : AbstractValidator<DescontarSaldoCommand>
{
    public DescontarSaldoCommandValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("A lista de itens não pode ser vazia.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ProdutoId).NotEmpty().WithMessage("O id do produto é obrigatório.");
            item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        });
    }
}
