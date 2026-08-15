using Korp.Faturamento.Application.Common.ExternalServices;
using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Domain.Errors;
using Korp.Faturamento.Domain.Repositories;
using Korp.SharedKernel.Results;
using MediatR;

namespace Korp.Faturamento.Application.NotasFiscais.Commands.ImprimirNotaFiscal;

internal sealed class ImprimirNotaFiscalCommandHandler
    : IRequestHandler<ImprimirNotaFiscalCommand, Result<NotaFiscalDto>>
{
    private readonly INotaFiscalRepository _repository;
    private readonly IEstoqueService _estoqueService;
    private readonly IIdempotencyService _idempotency;

    public ImprimirNotaFiscalCommandHandler(
        INotaFiscalRepository repository,
        IEstoqueService estoqueService,
        IIdempotencyService idempotency)
    {
        _repository = repository;
        _estoqueService = estoqueService;
        _idempotency = idempotency;
    }

    public async Task<Result<NotaFiscalDto>> Handle(
        ImprimirNotaFiscalCommand request,
        CancellationToken cancellationToken)
    {
        // Idempotência: retorna o resultado anterior se a chave já foi processada
        var cached = await _idempotency.GetAsync<NotaFiscalDto>(request.IdempotencyKey, cancellationToken);
        if (cached is not null)
            return Result.Success(cached);

        var nota = await _repository.GetByIdAsync(request.NotaFiscalId, cancellationToken);
        if (nota is null)
            return Result.Failure<NotaFiscalDto>(NotaFiscalErrors.NotFound(request.NotaFiscalId));

        if (nota.Status == Domain.Enums.StatusNota.Fechada)
            return Result.Failure<NotaFiscalDto>(NotaFiscalErrors.JaFechada(nota.Numero));

        var itens = nota.Itens
            .Select(i => new DescontarSaldoItem(i.ProdutoId, i.Quantidade))
            .ToList();

        try
        {
            var (sucesso, erroDetalhe) = await _estoqueService.DescontarSaldoAsync(itens, cancellationToken);

            if (!sucesso)
                return Result.Failure<NotaFiscalDto>(NotaFiscalErrors.SaldoInsuficiente(erroDetalhe!));
        }
        catch (EstoqueIndisponivelException)
        {
            return Result.Failure<NotaFiscalDto>(NotaFiscalErrors.EstoqueIndisponivel());
        }

        nota.Fechar();

        await _repository.UpdateAsync(nota, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new NotaFiscalDto(
            nota.Id,
            nota.Numero,
            nota.Status.ToString(),
            nota.ImpressoEm,
            nota.Itens.Select(i => new ItemNotaDto(i.Id, i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade)).ToList(),
            nota.CreatedAt);

        await _idempotency.SetAsync(request.IdempotencyKey, dto, cancellationToken);

        return Result.Success(dto);
    }
}
