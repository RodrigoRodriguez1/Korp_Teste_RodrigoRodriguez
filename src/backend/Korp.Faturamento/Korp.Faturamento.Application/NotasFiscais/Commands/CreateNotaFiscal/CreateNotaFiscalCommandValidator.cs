using FluentValidation;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.CreateNotaFiscal;

public sealed class CreateNotaFiscalCommandValidator : AbstractValidator<CreateNotaFiscalCommand>
{
    public CreateNotaFiscalCommandValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("A nota fiscal deve ter pelo menos um item.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ProdutoId).NotEmpty().WithMessage("O id do produto é obrigatório.");
            item.RuleFor(i => i.ProdutoCodigo).NotEmpty().WithMessage("O código do produto é obrigatório.");
            item.RuleFor(i => i.ProdutoDescricao).NotEmpty().WithMessage("A descrição do produto é obrigatória.");
            item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        });
    }
}
