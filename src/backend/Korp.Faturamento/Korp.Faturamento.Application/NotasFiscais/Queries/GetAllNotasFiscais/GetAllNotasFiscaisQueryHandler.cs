using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Queries.GetAllNotasFiscais;

internal sealed class GetAllNotasFiscaisQueryHandler
    : IRequestHandler<GetAllNotasFiscaisQuery, Result<List<NotaFiscalDto>>>
{
    private readonly INotaFiscalRepository _repository;

    public GetAllNotasFiscaisQueryHandler(INotaFiscalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<NotaFiscalDto>>> Handle(
        GetAllNotasFiscaisQuery request,
        CancellationToken cancellationToken)
    {
        var notas = await _repository.GetAllAsync(cancellationToken);
        var dtos = notas.Select(ToDto).ToList();
        return Result.Success(dtos);
    }

    private static NotaFiscalDto ToDto(Korp.Faturamento.Domain.Entities.NotaFiscal n) => new(
        n.Id,
        n.Numero,
        n.Status.ToString(),
        n.ImpressoEm,
        n.Itens.Select(i => new ItemNotaDto(i.Id, i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade)).ToList(),
        n.CreatedAt);
}
