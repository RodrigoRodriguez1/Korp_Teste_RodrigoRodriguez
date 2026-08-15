using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Domain.Errors;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Queries.GetProdutoById;

internal sealed class GetProdutoByIdQueryHandler
    : IRequestHandler<GetProdutoByIdQuery, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public GetProdutoByIdQueryHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProdutoDto>> Handle(
        GetProdutoByIdQuery request,
        CancellationToken cancellationToken)
    {
        var produto = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (produto is null)
            return Result.Failure<ProdutoDto>(ProdutoErrors.NotFound(request.Id));

        return Result.Success(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }
}
