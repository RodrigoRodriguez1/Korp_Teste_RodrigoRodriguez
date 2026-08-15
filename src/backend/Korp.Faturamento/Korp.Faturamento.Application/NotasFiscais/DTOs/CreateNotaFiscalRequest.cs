namespace Korp.Faturamento.Application.NotasFiscais.DTOs;

public sealed record CreateNotaFiscalRequest(List<ItemNotaRequest> Itens);

public sealed record ItemNotaRequest(
    Guid ProdutoId,
    string ProdutoCodigo,
    string ProdutoDescricao,
    int Quantidade);
