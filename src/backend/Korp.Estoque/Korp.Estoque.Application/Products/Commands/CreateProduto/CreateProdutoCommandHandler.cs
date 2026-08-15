using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Errors;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.CreateProduto;

internal sealed class CreateProdutoCommandHandler
    : IRequestHandler<CreateProdutoCommand, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _repository;

    public CreateProdutoCommandHandler(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProdutoDto>> Handle(
        CreateProdutoCommand request,
        CancellationToken cancellationToken)
    {
        var existe = await _repository.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (existe is not null)
            return Result.Failure<ProdutoDto>(ProdutoErrors.CodigoDuplicado(request.Codigo));

        var produto = Produto.Create(request.Codigo, request.Descricao, request.Saldo);
        await _repository.AddAsync(produto, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }
}
