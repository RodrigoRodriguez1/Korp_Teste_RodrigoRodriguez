using FluentAssertions;
using Korp.Faturamento.Application.NotasFiscais.Commands.CreateNotaFiscal;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Repositories;
using NSubstitute;

namespace Korp.Faturamento.UnitTests.Application;

public sealed class CreateNotaFiscalCommandHandlerTests
{
    private readonly INotaFiscalRepository _repository = Substitute.For<INotaFiscalRepository>();
    private readonly CreateNotaFiscalCommandHandler _handler;

    public CreateNotaFiscalCommandHandlerTests()
    {
        _handler = new CreateNotaFiscalCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ComItemValido_CriaNotaERetornaDto()
    {
        _repository.GetNextNumeroAsync(default).ReturnsForAnyArgs(42);

        var command = new CreateNotaFiscalCommand(
        [
            new CreateItemNotaCommand(Guid.NewGuid(), "P001", "Produto Alpha", 3)
        ]);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Numero.Should().Be(42);
        resultado.Value.Status.Should().Be("Aberta");
        resultado.Value.Itens.Should().HaveCount(1);
        resultado.Value.Itens[0].ProdutoCodigo.Should().Be("P001");
        resultado.Value.Itens[0].Quantidade.Should().Be(3);
        await _repository.Received(1).AddAsync(Arg.Any<NotaFiscal>(), default);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ComMultiplosItens_CriaNotaComTodosOsItens()
    {
        _repository.GetNextNumeroAsync(default).ReturnsForAnyArgs(7);

        var command = new CreateNotaFiscalCommand(
        [
            new CreateItemNotaCommand(Guid.NewGuid(), "P001", "Produto Alpha", 1),
            new CreateItemNotaCommand(Guid.NewGuid(), "P002", "Produto Beta", 5),
            new CreateItemNotaCommand(Guid.NewGuid(), "P003", "Produto Gamma", 10)
        ]);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Itens.Should().HaveCount(3);
        resultado.Value.Numero.Should().Be(7);
    }

    [Fact]
    public async Task Handle_NotaCriada_IdNaoEhVazio()
    {
        _repository.GetNextNumeroAsync(default).ReturnsForAnyArgs(1);

        var command = new CreateNotaFiscalCommand(
        [
            new CreateItemNotaCommand(Guid.NewGuid(), "P001", "Produto Alpha", 2)
        ]);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().NotBeEmpty();
        resultado.Value.ImpressoEm.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NotaCriada_CriadoEmEstaPreenchido()
    {
        _repository.GetNextNumeroAsync(default).ReturnsForAnyArgs(99);

        var command = new CreateNotaFiscalCommand(
        [
            new CreateItemNotaCommand(Guid.NewGuid(), "P001", "Produto Alpha", 1)
        ]);

        var antes = DateTime.UtcNow;
        var resultado = await _handler.Handle(command, CancellationToken.None);
        var depois = DateTime.UtcNow;

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.CriadoEm.Should().BeOnOrAfter(antes).And.BeOnOrBefore(depois);
    }
}
