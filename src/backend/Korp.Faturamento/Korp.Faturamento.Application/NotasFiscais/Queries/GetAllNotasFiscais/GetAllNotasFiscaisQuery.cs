using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Queries.GetAllNotasFiscais;

public sealed record GetAllNotasFiscaisQuery : IRequest<Result<List<NotaFiscalDto>>>;
