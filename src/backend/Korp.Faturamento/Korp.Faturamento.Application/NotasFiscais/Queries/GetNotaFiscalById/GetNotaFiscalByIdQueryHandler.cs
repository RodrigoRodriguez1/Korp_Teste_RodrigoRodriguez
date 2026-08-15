using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Domain.Errors;
using Korp.Faturamento.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Queries.GetNotaFiscalById;

internal sealed class GetNotaFiscalByIdQueryHandler
    : IRequestHandler<GetNotaFiscalByIdQuery, Result<NotaFiscalDto>>
{
    private readonly INotaFiscalRepository _repository;

    public GetNotaFiscalByIdQueryHandler(INotaFiscalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NotaFiscalDto>> Handle(
        GetNotaFiscalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var nota = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (nota is null)
            return Result.Failure<NotaFiscalDto>(NotaFiscalErrors.NotFound(request.Id));

        var dto = new NotaFiscalDto(
            nota.Id,
            nota.Numero,
            nota.Status.ToString(),
            nota.ImpressoEm,
            nota.Itens.Select(i => new ItemNotaDto(i.Id, i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade)).ToList(),
            nota.CreatedAt);

        return Result.Success(dto);
    }
}
