using Korp.SharedKernel.Results;

namespace Korp.Faturamento.Domain.Errors;

public static class NotaFiscalErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("NotaFiscal.NotFound", $"Nota fiscal com id '{id}' não foi encontrada.");

    public static Error JaFechada(int numero) =>
        Error.Conflict("NotaFiscal.JaFechada",
            $"A nota fiscal {numero} já está fechada e não pode ser impressa novamente.");

    public static Error EstoqueIndisponivel() =>
        Error.ServiceUnavailable("NotaFiscal.EstoqueIndisponivel",
            "O serviço de estoque está temporariamente indisponível. Sua nota fiscal não foi alterada. Tente novamente em alguns segundos.");

    public static Error SaldoInsuficiente(string detalhe) =>
        Error.Conflict("NotaFiscal.SaldoInsuficiente", detalhe);

    public static Error ListaItensVazia() =>
        Error.Validation("NotaFiscal.ListaItensVazia", "A nota fiscal deve ter pelo menos um item.");
}
