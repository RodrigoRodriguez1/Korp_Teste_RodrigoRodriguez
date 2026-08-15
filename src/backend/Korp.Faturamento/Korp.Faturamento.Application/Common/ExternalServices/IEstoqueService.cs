namespace Korp.Faturamento.Application.Common.ExternalServices;

public sealed record DescontarSaldoItem(Guid ProdutoId, int Quantidade);

public interface IEstoqueService
{
    /// <summary>
    /// Desconta o saldo dos produtos no microsserviço de Estoque.
    /// Lança EstoqueIndisponivelException se o serviço estiver inacessível.
    /// Retorna false se houver saldo insuficiente (com detalhes no out param).
    /// </summary>
    Task<(bool Sucesso, string? ErroDetalhe)> DescontarSaldoAsync(
        List<DescontarSaldoItem> itens,
        CancellationToken cancellationToken = default);
}
