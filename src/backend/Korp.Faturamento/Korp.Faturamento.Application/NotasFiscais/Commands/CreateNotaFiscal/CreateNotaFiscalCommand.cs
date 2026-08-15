using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.CreateNotaFiscal;

public sealed record CreateNotaFiscalCommand(List<CreateItemNotaCommand> Itens)
    : IRequest<Result<NotaFiscalDto>>;

public sealed record CreateItemNotaCommand(
    Guid ProdutoId,
    string ProdutoCodigo,
    string ProdutoDescricao,
    int Quantidade);
