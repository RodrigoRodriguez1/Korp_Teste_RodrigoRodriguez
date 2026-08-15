using FluentAssertions;
using Korp.Estoque.Application.Products.Commands.DescontarSaldo;
using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using NSubstitute;

namespace Korp.Estoque.UnitTests.Application;

public sealed class DescontarSaldoCommandHandlerTests
{
    private readonly IProdutoRepository _repository = Substitute.For<IProdutoRepository>();
    private readonly DescontarSaldoCommandHandler _handler;

    public DescontarSaldoCommandHandlerTests()
    {
        _handler = new DescontarSaldoCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ComSaldoSuficiente_DescontaERetornaSuccess()
    {
        var produtoId = Guid.NewGuid();
        var produto = Produto.Create("P001", "Produto Teste", 10);

        // Usa reflexão para setar o Id (campo privado)
        typeof(Produto).GetProperty("Id")!.SetValue(produto, produtoId);
        _repository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), default).Returns([produto]);

        var command = new DescontarSaldoCommand([new DescontarSaldoItemDto(produtoId, 3)]);
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        produto.Saldo.Should().Be(7);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_QuandoSaldoInsuficiente_RetornaConflictSemSalvarNada()
    {
        var produtoId = Guid.NewGuid();
        var produto = Produto.Create("P001", "Produto Teste", 1);
        typeof(Produto).GetProperty("Id")!.SetValue(produto, produtoId);
        _repository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), default).Returns([produto]);

        var command = new DescontarSaldoCommand([new DescontarSaldoItemDto(produtoId, 5)]);
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        resultado.Error.Code.Should().Be("Produto.SaldoInsuficiente");
        produto.Saldo.Should().Be(1);
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_QuandoProdutoNaoEncontrado_RetornaNotFound()
    {
        _repository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), default).Returns([]);

        var command = new DescontarSaldoCommand([new DescontarSaldoItemDto(Guid.NewGuid(), 1)]);
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
