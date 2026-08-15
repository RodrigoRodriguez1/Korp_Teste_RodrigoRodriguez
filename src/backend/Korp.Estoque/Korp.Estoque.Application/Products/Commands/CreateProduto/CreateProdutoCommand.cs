using Korp.Estoque.Application.Products.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.CreateProduto;

public sealed record CreateProdutoCommand(string Codigo, string Descricao, int Saldo)
    : IRequest<Result<ProdutoDto>>;
