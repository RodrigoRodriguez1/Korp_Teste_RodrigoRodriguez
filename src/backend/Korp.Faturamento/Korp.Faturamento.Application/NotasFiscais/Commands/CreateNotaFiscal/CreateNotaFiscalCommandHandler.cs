using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.CreateNotaFiscal;

internal sealed class CreateNotaFiscalCommandHandler
    : IRequestHandler<CreateNotaFiscalCommand, Result<NotaFiscalDto>>
{
    private readonly INotaFiscalRepository _repository;

    public CreateNotaFiscalCommandHandler(INotaFiscalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NotaFiscalDto>> Handle(
        CreateNotaFiscalCommand request,
        CancellationToken cancellationToken)
    {
        var numero = await _repository.GetNextNumeroAsync(cancellationToken);

        var nota = NotaFiscal.Create();
        nota.SetNumero(numero);

        foreach (var item in request.Itens)
            nota.AdicionarItem(item.ProdutoId, item.ProdutoCodigo, item.ProdutoDescricao, item.Quantidade);

        await _repository.AddAsync(nota, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

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
