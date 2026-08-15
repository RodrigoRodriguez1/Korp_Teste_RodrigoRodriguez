using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.DeleteProduto;

public sealed record DeleteProdutoCommand(Guid Id) : IRequest<Result>;
