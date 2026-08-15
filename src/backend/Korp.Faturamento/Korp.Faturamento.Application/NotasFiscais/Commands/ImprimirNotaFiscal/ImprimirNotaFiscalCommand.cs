using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.ImprimirNotaFiscal;

public sealed record ImprimirNotaFiscalCommand(Guid NotaFiscalId, string IdempotencyKey)
    : IRequest<Result<NotaFiscalDto>>;
