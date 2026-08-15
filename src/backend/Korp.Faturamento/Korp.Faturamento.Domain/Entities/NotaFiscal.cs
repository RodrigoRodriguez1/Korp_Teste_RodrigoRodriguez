using Korp.Faturamento.Domain.Enums;
using Korp.Faturamento.Domain.Errors;
using Korp.SharedKernel.Results;

namespace Korp.Faturamento.Domain.Entities;

public sealed class NotaFiscal
{
    private readonly List<ItemNota> _itens = [];

    private NotaFiscal() { }

    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public StatusNota Status { get; private set; }
    public DateTime? ImpressoEm { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<ItemNota> Itens => _itens.AsReadOnly();

    public static NotaFiscal Create() => new()
    {
        Id = Guid.NewGuid(),
        Status = StatusNota.Aberta,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public void AdicionarItem(
        Guid produtoId,
        string produtoCodigo,
        string produtoDescricao,
        int quantidade)
    {
        _itens.Add(ItemNota.Create(Id, produtoId, produtoCodigo, produtoDescricao, quantidade));
        UpdatedAt = DateTime.UtcNow;
    }

    public Result Fechar()
    {
        if (Status == StatusNota.Fechada)
            return Result.Failure(NotaFiscalErrors.JaFechada(Numero));

        Status = StatusNota.Fechada;
        ImpressoEm = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // Chamado pela Infrastructure após persistir (a sequence do PG define o número)
    public void SetNumero(int numero) => Numero = numero;
}
