using Korp.Estoque.Application.Products.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Estoque.Application.Products.Commands.DescontarSaldo;

public sealed record DescontarSaldoCommand(List<DescontarSaldoItemDto> Itens) : IRequest<Result>;
