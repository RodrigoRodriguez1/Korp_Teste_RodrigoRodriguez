using FluentAssertions;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Enums;
using Korp.SharedKernel.Results;

namespace Korp.Faturamento.UnitTests.Domain;

public sealed class NotaFiscalTests
{
    [Fact]
    public void Create_RetornaNotaAbertaComIdValido()
    {
        var nota = NotaFiscal.Create();

        nota.Id.Should().NotBeEmpty();
        nota.Status.Should().Be(StatusNota.Aberta);
        nota.ImpressoEm.Should().BeNull();
        nota.Itens.Should().BeEmpty();
        nota.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AdicionarItem_AdicionaItemNaLista()
    {
        var nota = NotaFiscal.Create();
        var produtoId = Guid.NewGuid();

        nota.AdicionarItem(produtoId, "P001", "Produto Teste", 3);

        nota.Itens.Should().HaveCount(1);
        nota.Itens.First().ProdutoId.Should().Be(produtoId);
        nota.Itens.First().Quantidade.Should().Be(3);
    }

    [Fact]
    public void AdicionarMultiplosItens_AdicionaTodosNaLista()
    {
        var nota = NotaFiscal.Create();

        nota.AdicionarItem(Guid.NewGuid(), "P001", "Produto A", 2);
        nota.AdicionarItem(Guid.NewGuid(), "P002", "Produto B", 5);

        nota.Itens.Should().HaveCount(2);
    }

    [Fact]
    public void Fechar_QuandoAberta_FechaESetaImpressoEm()
    {
        var nota = NotaFiscal.Create();

        var resultado = nota.Fechar();

        resultado.IsSuccess.Should().BeTrue();
        nota.Status.Should().Be(StatusNota.Fechada);
        nota.ImpressoEm.Should().NotBeNull();
        nota.ImpressoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Fechar_QuandoJaFechada_RetornaFailureComCodigoCorreto()
    {
        var nota = NotaFiscal.Create();
        nota.Fechar();

        var resultado = nota.Fechar();

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Code.Should().Be("NotaFiscal.JaFechada");
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void SetNumero_AtribuiNumeroCorretamente()
    {
        var nota = NotaFiscal.Create();

        nota.SetNumero(42);

        nota.Numero.Should().Be(42);
    }
}
