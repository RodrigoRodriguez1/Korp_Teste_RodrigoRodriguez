using Korp.SharedKernel.Results;

namespace Korp.Estoque.Domain.Errors;

public static class ProdutoErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Produto.NotFound", $"Produto com id '{id}' não foi encontrado.");

    public static Error CodigoDuplicado(string codigo) =>
        Error.Conflict("Produto.CodigoDuplicado", $"Já existe um produto com o código '{codigo}'.");

    public static Error SaldoInsuficiente(string codigo, int saldoAtual, int quantidadeSolicitada) =>
        Error.Conflict("Produto.SaldoInsuficiente",
            $"Produto '{codigo}' possui saldo {saldoAtual}, mas a nota requer {quantidadeSolicitada} unidades.");

    public static Error EmUsoEmNotaAberta(string codigo) =>
        Error.Conflict("Produto.EmUsoEmNotaAberta",
            $"Produto '{codigo}' está em uso em uma nota fiscal com status Aberta e não pode ser excluído.");
}
