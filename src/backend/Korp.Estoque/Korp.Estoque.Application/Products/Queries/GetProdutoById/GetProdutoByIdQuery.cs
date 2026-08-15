using Korp.Estoque.Application.Products.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Queries.GetProdutoById;

public sealed record GetProdutoByIdQuery(Guid Id) : IRequest<Result<ProdutoDto>>;
