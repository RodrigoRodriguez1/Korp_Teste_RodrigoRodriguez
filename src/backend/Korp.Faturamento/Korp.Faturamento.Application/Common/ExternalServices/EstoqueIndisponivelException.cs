namespace Korp.Faturamento.Application.Common.ExternalServices;

public sealed class EstoqueIndisponivelException : Exception
{
    public EstoqueIndisponivelException()
        : base("O serviço de estoque está temporariamente indisponível.") { }

    public EstoqueIndisponivelException(Exception innerException)
        : base("O serviço de estoque está temporariamente indisponível.", innerException) { }
}
