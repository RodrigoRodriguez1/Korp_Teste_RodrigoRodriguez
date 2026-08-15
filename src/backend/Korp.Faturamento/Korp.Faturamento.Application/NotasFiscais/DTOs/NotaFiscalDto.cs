namespace Korp.Faturamento.Application.NotasFiscais.DTOs;

public sealed record NotaFiscalDto(
    Guid Id,
    int Numero,
    string Status,
    DateTime? ImpressoEm,
    List<ItemNotaDto> Itens,
    DateTime CriadoEm);

public sealed record ItemNotaDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoCodigo,
    string ProdutoDescricao,
    int Quantidade);
