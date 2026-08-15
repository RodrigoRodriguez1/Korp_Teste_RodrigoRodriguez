namespace Korp.Estoque.Application.Products.DTOs;

public sealed record ProdutoDto(Guid Id, string Codigo, string Descricao, int Saldo);
