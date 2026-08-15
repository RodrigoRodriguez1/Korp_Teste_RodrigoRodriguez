using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Queries.GetNotaFiscalById;

public sealed record GetNotaFiscalByIdQuery(Guid Id) : IRequest<Result<NotaFiscalDto>>;
