using FluentAssertions;
using Korp.Estoque.Domain.Entities;
using Korp.SharedKernel.Results;

namespace Korp.Estoque.UnitTests.Domain;

public sealed class ProdutoTests
{
    [Fact]
    public void Create_ComDadosValidos_RetornaProdutoCorreto()
    {
        var produto = Produto.Create("P001", "Produto Teste", 10);

        produto.Codigo.Should().Be("P001");
        produto.Descricao.Should().Be("Produto Teste");
        produto.Saldo.Should().Be(10);
        produto.Id.Should().NotBeEmpty();
        produto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DescontarSaldo_QuandoQuantidadeValida_DecrementaSaldoERetornaSuccess()
    {
        var produto = Produto.Create("P001", "Produto Teste", 10);

        var resultado = produto.DescontarSaldo(3);

        resultado.IsSuccess.Should().BeTrue();
        produto.Saldo.Should().Be(7);
    }

    [Fact]
    public void DescontarSaldo_QuandoQuantidadeIgualAoSaldo_ZeraSaldoERetornaSuccess()
    {
        var produto = Produto.Create("P001", "Produto Teste", 5);

        var resultado = produto.DescontarSaldo(5);

        resultado.IsSuccess.Should().BeTrue();
        produto.Saldo.Should().Be(0);
    }

    [Fact]
    public void DescontarSaldo_QuandoQuantidadeMaiorQueSaldo_RetornaFailureSemAlterarSaldo()
    {
        var produto = Produto.Create("P001", "Produto Teste", 5);

        var resultado = produto.DescontarSaldo(10);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        resultado.Error.Code.Should().Be("Produto.SaldoInsuficiente");
        produto.Saldo.Should().Be(5);
    }

    [Fact]
    public void DescontarSaldo_QuandoSaldoZero_RetornaFailure()
    {
        var produto = Produto.Create("P001", "Produto Teste", 0);

        var resultado = produto.DescontarSaldo(1);

        resultado.IsFailure.Should().BeTrue();
        produto.Saldo.Should().Be(0);
    }

    [Fact]
    public void Update_AtualizaPropriedadesCorretamente()
    {
        var produto = Produto.Create("P001", "Descricao Original", 10);

        produto.Update("P002", "Nova Descricao", 20);

        produto.Codigo.Should().Be("P002");
        produto.Descricao.Should().Be("Nova Descricao");
        produto.Saldo.Should().Be(20);
        produto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
