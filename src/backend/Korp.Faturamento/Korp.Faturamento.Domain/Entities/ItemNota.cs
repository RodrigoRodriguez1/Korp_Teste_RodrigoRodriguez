namespace Korp.Faturamento.Domain.Entities;

public sealed class ItemNota
{
    private ItemNota() { }

    public Guid Id { get; private set; }
    public Guid NotaFiscalId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string ProdutoCodigo { get; private set; } = string.Empty;
    public string ProdutoDescricao { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }

    internal static ItemNota Create(
        Guid notaFiscalId,
        Guid produtoId,
        string produtoCodigo,
        string produtoDescricao,
        int quantidade) => new()
    {
        Id = Guid.NewGuid(),
        NotaFiscalId = notaFiscalId,
        ProdutoId = produtoId,
        ProdutoCodigo = produtoCodigo,
        ProdutoDescricao = produtoDescricao,
        Quantidade = quantidade
    };
}
