using Korp.Estoque.Application.Products.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.UpdateProduto;

public sealed record UpdateProdutoCommand(Guid Id, string Codigo, string Descricao, int Saldo)
    : IRequest<Result<ProdutoDto>>;
