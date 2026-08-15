using Korp.Estoque.Domain.Errors;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.DescontarSaldo;

internal sealed class DescontarSaldoCommandHandler : IRequestHandler<DescontarSaldoCommand, Result>
{
    private readonly IProdutoRepository _repository;

    public DescontarSaldoCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DescontarSaldoCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Itens.Select(i => i.ProdutoId).ToList();

        // Busca todos os produtos com lock pessimista (FOR UPDATE) na Infrastructure
        var produtos = await _repository.GetByIdsAsync(ids, cancellationToken);

        // Valida existência de todos antes de descontar qualquer um
        foreach (var item in request.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto is null)
                return Result.Failure(ProdutoErrors.NotFound(item.ProdutoId));
        }

        // Valida saldo de todos antes de descontar qualquer um (atomicidade)
        foreach (var item in request.Itens)
        {
            var produto = produtos.First(p => p.Id == item.ProdutoId);
            if (item.Quantidade > produto.Saldo)
                return Result.Failure(ProdutoErrors.SaldoInsuficiente(produto.Codigo, produto.Saldo, item.Quantidade));
        }

        // Desconta — todos os produtos passaram na validação
        foreach (var item in request.Itens)
        {
            var produto = produtos.First(p => p.Id == item.ProdutoId);
            produto.DescontarSaldo(item.Quantidade);
            await _repository.UpdateAsync(produto, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
