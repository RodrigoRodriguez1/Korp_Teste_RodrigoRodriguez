using Korp.Estoque.Domain.Errors;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.DeleteProduto;

internal sealed class DeleteProdutoCommandHandler : IRequestHandler<DeleteProdutoCommand, Result>
{
    private readonly IProdutoRepository _repository;

    public DeleteProdutoCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (produto is null)
            return Result.Failure(ProdutoErrors.NotFound(request.Id));

        await _repository.DeleteAsync(produto, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
