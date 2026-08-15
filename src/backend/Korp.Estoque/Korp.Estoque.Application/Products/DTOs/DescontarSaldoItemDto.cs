namespace Korp.Estoque.Application.Products.DTOs;

public sealed record DescontarSaldoItemDto(Guid ProdutoId, int Quantidade);

public sealed record DescontarSaldoRequestDto(List<DescontarSaldoItemDto> Itens);
