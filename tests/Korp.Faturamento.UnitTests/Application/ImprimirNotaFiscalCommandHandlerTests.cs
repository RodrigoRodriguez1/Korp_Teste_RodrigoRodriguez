using FluentAssertions;
using Korp.Faturamento.Application.Common.ExternalServices;
using Korp.Faturamento.Application.NotasFiscais.Commands.ImprimirNotaFiscal;
using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Enums;
using Korp.Faturamento.Domain.Repositories;
using Korp.SharedKernel.Results;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Korp.Faturamento.UnitTests.Application;

public sealed class ImprimirNotaFiscalCommandHandlerTests
{
    private readonly INotaFiscalRepository _repository = Substitute.For<INotaFiscalRepository>();
    private readonly IEstoqueService _estoqueService = Substitute.For<IEstoqueService>();
    private readonly IIdempotencyService _idempotency = Substitute.For<IIdempotencyService>();
    private readonly ImprimirNotaFiscalCommandHandler _handler;

    public ImprimirNotaFiscalCommandHandlerTests()
    {
        _handler = new ImprimirNotaFiscalCommandHandler(_repository, _estoqueService, _idempotency);
    }

    [Fact]
    public async Task Handle_QuandoEstoqueSuficiente_FechaNotaERetornaDto()
    {
        var nota = NotaFiscal.Create();
        nota.AdicionarItem(Guid.NewGuid(), "P001", "Produto A", 2);
        nota.SetNumero(1);

        _idempotency.GetAsync<NotaFiscalDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscalDto?)null);
        _repository.GetByIdAsync(nota.Id, Arg.Any<CancellationToken>()).Returns(nota);
        _estoqueService.DescontarSaldoAsync(Arg.Any<List<DescontarSaldoItem>>(), Arg.Any<CancellationToken>())
            .Returns((true, null));

        var command = new ImprimirNotaFiscalCommand(nota.Id, "idempotency-key-1");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Status.Should().Be("Fechada");
        resultado.Value.ImpressoEm.Should().NotBeNull();
        await _repository.Received(1).UpdateAsync(nota, Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _idempotency.Received(1).SetAsync(
            "idempotency-key-1", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoNotaNaoEncontrada_RetornaNotFound()
    {
        _idempotency.GetAsync<NotaFiscalDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscalDto?)null);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscal?)null);

        var command = new ImprimirNotaFiscalCommand(Guid.NewGuid(), "key");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.NotFound);
        resultado.Error.Code.Should().Be("NotaFiscal.NotFound");
    }

    [Fact]
    public async Task Handle_QuandoNotaJaFechada_RetornaConflict()
    {
        var nota = NotaFiscal.Create();
        nota.SetNumero(1);
        nota.Fechar();

        _idempotency.GetAsync<NotaFiscalDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscalDto?)null);
        _repository.GetByIdAsync(nota.Id, Arg.Any<CancellationToken>()).Returns(nota);

        var command = new ImprimirNotaFiscalCommand(nota.Id, "key");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        resultado.Error.Code.Should().Be("NotaFiscal.JaFechada");
        await _estoqueService.DidNotReceive()
            .DescontarSaldoAsync(Arg.Any<List<DescontarSaldoItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoEstoqueIndisponivel_RetornaServiceUnavailableSemFecharNota()
    {
        var nota = NotaFiscal.Create();
        nota.AdicionarItem(Guid.NewGuid(), "P001", "Produto A", 1);
        nota.SetNumero(2);

        _idempotency.GetAsync<NotaFiscalDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscalDto?)null);
        _repository.GetByIdAsync(nota.Id, Arg.Any<CancellationToken>()).Returns(nota);
        _estoqueService.DescontarSaldoAsync(Arg.Any<List<DescontarSaldoItem>>(), Arg.Any<CancellationToken>())
            .Throws(new EstoqueIndisponivelException());

        var command = new ImprimirNotaFiscalCommand(nota.Id, "key");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
        resultado.Error.Code.Should().Be("NotaFiscal.EstoqueIndisponivel");
        nota.Status.Should().Be(StatusNota.Aberta);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoSaldoInsuficiente_RetornaConflictSemFecharNota()
    {
        var nota = NotaFiscal.Create();
        nota.AdicionarItem(Guid.NewGuid(), "P001", "Produto A", 999);
        nota.SetNumero(3);

        _idempotency.GetAsync<NotaFiscalDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotaFiscalDto?)null);
        _repository.GetByIdAsync(nota.Id, Arg.Any<CancellationToken>()).Returns(nota);
        _estoqueService.DescontarSaldoAsync(Arg.Any<List<DescontarSaldoItem>>(), Arg.Any<CancellationToken>())
            .Returns((false, "Saldo insuficiente para o produto P001: solicitado 999, disponível 5."));

        var command = new ImprimirNotaFiscalCommand(nota.Id, "key");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        resultado.Error.Code.Should().Be("NotaFiscal.SaldoInsuficiente");
        nota.Status.Should().Be(StatusNota.Aberta);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoChaveIdempotenteDuplicada_RetornaResultadoArmazenado()
    {
        var notaId = Guid.NewGuid();
        var dto = new NotaFiscalDto(
            notaId, 1, "Fechada", DateTime.UtcNow, [], DateTime.UtcNow);

        _idempotency.GetAsync<NotaFiscalDto>(
            "key-duplicada", Arg.Any<CancellationToken>()).Returns(dto);

        var command = new ImprimirNotaFiscalCommand(notaId, "key-duplicada");
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeEquivalentTo(dto);
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _estoqueService.DidNotReceive()
            .DescontarSaldoAsync(Arg.Any<List<DescontarSaldoItem>>(), Arg.Any<CancellationToken>());
    }
}
