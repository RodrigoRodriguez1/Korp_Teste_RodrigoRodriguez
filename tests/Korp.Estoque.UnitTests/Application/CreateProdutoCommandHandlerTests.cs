using FluentAssertions;
using Korp.Estoque.Application.Products.Commands.CreateProduto;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Repositories;
using Korp.SharedKernel.Results;
using NSubstitute;

namespace Korp.Estoque.UnitTests.Application;

public sealed class CreateProdutoCommandHandlerTests
{
    private readonly IProdutoRepository _repository = Substitute.For<IProdutoRepository>();
    private readonly CreateProdutoCommandHandler _handler;

    public CreateProdutoCommandHandlerTests()
    {
        _handler = new CreateProdutoCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_CriaProdutoERetornaSuccess()
    {
        _repository.GetByCodigoAsync("P001", default).Returns((Produto?)null);

        var command = new CreateProdutoCommand("P001", "Produto Teste", 10);
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Codigo.Should().Be("P001");
        resultado.Value.Descricao.Should().Be("Produto Teste");
        resultado.Value.Saldo.Should().Be(10);
        await _repository.Received(1).AddAsync(Arg.Any<Produto>(), default);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_QuandoCodigoJaExiste_RetornaConflict()
    {
        var produtoExistente = Produto.Create("P001", "Existente", 5);
        _repository.GetByCodigoAsync("P001", default).Returns(produtoExistente);

        var command = new CreateProdutoCommand("P001", "Novo Produto", 10);
        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        resultado.Error.Code.Should().Be("Produto.CodigoDuplicado");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Produto>(), default);
    }
}
