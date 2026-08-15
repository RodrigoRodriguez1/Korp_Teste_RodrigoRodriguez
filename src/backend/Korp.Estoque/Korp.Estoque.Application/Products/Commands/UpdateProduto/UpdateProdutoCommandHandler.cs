using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Domain.Errors;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.UpdateProduto;

internal sealed class UpdateProdutoCommandHandler
    : IRequestHandler<UpdateProdutoCommand, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public UpdateProdutoCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProdutoDto>> Handle(
        UpdateProdutoCommand request,
        CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (produto is null)
            return Result.Failure<ProdutoDto>(ProdutoErrors.NotFound(request.Id));

        var duplicado = await _repository.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (duplicado is not null && duplicado.Id != request.Id)
            return Result.Failure<ProdutoDto>(ProdutoErrors.CodigoDuplicado(request.Codigo));

        produto.Update(request.Codigo, request.Descricao, request.Saldo);
        await _repository.UpdateAsync(produto, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }
}
