# 09 — Estratégia de Testes

## Princípios

- Testes unitários obrigatórios para toda lógica de negócio (handlers, entidades)
- Testes de integração para endpoints críticos (imprimir NF, descontar saldo)
- Sem mock de banco de dados — testes de integração usam PostgreSQL real via Docker
- Cobertura mínima de 80% nos handlers de Application

## .NET — Testes Unitários

### Stack
- **xUnit** — framework de testes
- **NSubstitute** — mocking de interfaces (preferido ao Moq pela sintaxe mais limpa)
- **FluentAssertions** — asserções legíveis

### Estrutura dos projetos de teste

```
tests/
├── Korp.Estoque.UnitTests/
│   ├── Domain/
│   │   └── ProdutoTests.cs          # Testa Produto.DescontarSaldo(), Create()
│   └── Application/
│       ├── CreateProdutoHandlerTests.cs
│       ├── DescontarSaldoHandlerTests.cs
│       └── GetAllProdutosHandlerTests.cs
│
└── Korp.Faturamento.UnitTests/
    ├── Domain/
    │   └── NotaFiscalTests.cs        # Testa NotaFiscal.Fechar(), AdicionarItem()
    └── Application/
        ├── CreateNotaFiscalHandlerTests.cs
        └── ImprimirNotaFiscalHandlerTests.cs
```

### Convenção de nomenclatura de testes

```
Método_Cenário_ResultadoEsperado

DescontarSaldo_QuandoQuantidadeMaiorQueSaldo_RetornaFailure
DescontarSaldo_QuandoQuantidadeValida_RetornaSuccessEAtualizaSaldo
Fechar_QuandoNotaJaFechada_RetornaFailure
Fechar_QuandoNotaAberta_RetornaSuccessEAtualizaStatus
```

### Exemplo de teste unitário

```csharp
public class ProdutoTests
{
    [Fact]
    public void DescontarSaldo_QuandoQuantidadeMaiorQueSaldo_RetornaFailure()
    {
        // Arrange
        var produto = Produto.Create("P001", "Produto Teste", saldo: 5);

        // Act
        var resultado = produto.DescontarSaldo(quantidade: 10);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        produto.Saldo.Should().Be(5); // saldo não alterado
    }

    [Fact]
    public void DescontarSaldo_QuandoQuantidadeValida_DecrementaSaldo()
    {
        var produto = Produto.Create("P001", "Produto Teste", saldo: 10);

        var resultado = produto.DescontarSaldo(3);

        resultado.IsSuccess.Should().BeTrue();
        produto.Saldo.Should().Be(7);
    }
}
```

```csharp
public class ImprimirNotaFiscalHandlerTests
{
    private readonly INotaFiscalRepository _repository = Substitute.For<INotaFiscalRepository>();
    private readonly IEstoqueService _estoqueService = Substitute.For<IEstoqueService>();

    [Fact]
    public async Task Handle_QuandoNotaFechada_RetornaFailure()
    {
        // Arrange
        var nota = NotaFiscal.Create();
        nota.Fechar(); // já fechada
        _repository.GetByIdAsync(nota.Id, default).Returns(nota);

        var handler = new ImprimirNotaFiscalCommandHandler(_repository, _estoqueService);
        var command = new ImprimirNotaFiscalCommand(nota.Id);

        // Act
        var resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.Conflict);
        await _estoqueService.DidNotReceive().DescontarSaldoAsync(default!, default);
    }

    [Fact]
    public async Task Handle_QuandoEstoqueIndisponivel_RetornaServiceUnavailableENaoFechaNota()
    {
        var nota = CriarNotaAbertaComItens();
        _repository.GetByIdAsync(nota.Id, default).Returns(nota);
        _estoqueService.DescontarSaldoAsync(default!, default)
            .ThrowsAsync(new EstoqueIndisponivelException());

        var handler = new ImprimirNotaFiscalCommandHandler(_repository, _estoqueService);
        var resultado = await handler.Handle(new ImprimirNotaFiscalCommand(nota.Id), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
        nota.Status.Should().Be(StatusNota.Aberta); // NÃO fechou
        await _repository.DidNotReceive().UpdateAsync(default!, default);
    }
}
```

## .NET — Testes de Integração

```
tests/
├── Korp.Estoque.IntegrationTests/
│   └── ProdutosEndpointsTests.cs
└── Korp.Faturamento.IntegrationTests/
    └── NotasFiscaisEndpointsTests.cs
```

- Usa `WebApplicationFactory<Program>` com banco PostgreSQL real (docker no CI)
- Cada test class cria/limpa seus dados para isolamento
- Testa os endpoints ponta-a-ponta incluindo validação e códigos HTTP

```csharp
public class ProdutosEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task PostProdutos_ComDadosValidos_Retorna201()
    {
        var request = new { codigo = "P001", descricao = "Produto Teste", saldo = 10 };
        var response = await _client.PostAsJsonAsync("/produtos", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostProdutos_ComCodigoDuplicado_Retorna409()
    {
        // cria produto P001
        // tenta criar outro P001
        // espera 409
    }
}
```

## Angular — Testes Unitários

- **Jasmine + Karma** (padrão Angular CLI)
- Foco em: serviços, pipes e lógica de componentes (não DOM)

```typescript
// produto.service.spec.ts
describe('ProdutoService', () => {
  it('deve retornar lista de produtos ao chamar getProdutos()', () => {
    const mockProdutos: Produto[] = [{ id: '1', codigo: 'P001', descricao: 'Teste', saldo: 5 }];
    httpMock.expectOne('/produtos').flush(mockProdutos);
    service.getProdutos().subscribe(produtos => {
      expect(produtos.length).toBe(1);
      expect(produtos[0].codigo).toBe('P001');
    });
  });
});
```

## Casos de teste obrigatórios por feature

### Feature: Cadastro de Produto
- [ ] Criar produto com dados válidos → sucesso
- [ ] Criar produto com código duplicado → erro 409
- [ ] Criar produto com saldo negativo → erro 400
- [ ] Criar produto sem código → erro 400
- [ ] Editar produto existente → sucesso
- [ ] Excluir produto sem NF associada → sucesso
- [ ] Excluir produto com NF Aberta associada → erro 409

### Feature: Cadastro de NF
- [ ] Criar NF com itens válidos → sucesso, status Aberta, número sequencial
- [ ] Criar NF sem itens → erro 400
- [ ] Criar NF com quantidade zero → erro 400

### Feature: Impressão de NF
- [ ] Imprimir NF Aberta com saldo suficiente → sucesso, status Fechada, saldo decrementado
- [ ] Imprimir NF Fechada → erro 409
- [ ] Imprimir NF com saldo insuficiente → erro 409, NF permanece Aberta, saldo não alterado
- [ ] Imprimir NF com Estoque offline → erro 503, NF permanece Aberta
- [ ] Imprimir NF duas vezes com mesma Idempotency-Key → mesmo resultado, saldo descontado apenas 1x

### Feature: Concorrência (opcional)
- [ ] Dois requests simultâneos para produto com saldo 1 → apenas 1 sucede, outro recebe 409
