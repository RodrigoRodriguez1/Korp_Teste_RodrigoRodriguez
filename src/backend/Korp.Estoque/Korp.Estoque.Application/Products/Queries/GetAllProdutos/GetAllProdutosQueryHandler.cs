using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Queries.GetAllProdutos;

internal sealed class GetAllProdutosQueryHandler
    : IRequestHandler<GetAllProdutosQuery, Result<List<ProdutoDto>>>
{
    private readonly IProdutoRepository _repository;

    public GetAllProdutosQueryHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ProdutoDto>>> Handle(
        GetAllProdutosQuery request,
        CancellationToken cancellationToken)
    {
        var produtos = await _repository.GetAllAsync(cancellationToken);
        var dtos = produtos.Select(p => new ProdutoDto(p.Id, p.Codigo, p.Descricao, p.Saldo)).ToList();
        return Result.Success(dtos);
    }
}
