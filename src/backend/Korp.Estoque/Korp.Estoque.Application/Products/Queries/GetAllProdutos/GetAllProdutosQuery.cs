using Korp.Estoque.Application.Products.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Queries.GetAllProdutos;

public sealed record GetAllProdutosQuery : IRequest<Result<List<ProdutoDto>>>;
