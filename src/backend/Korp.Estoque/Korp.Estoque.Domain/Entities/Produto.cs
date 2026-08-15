using Korp.Estoque.Domain.Errors;
using Korp.SharedKernel.Results;

namespace Korp.Estoque.Domain.Entities;

public sealed class Produto
{
    private Produto() { }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public int Saldo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Produto Create(string codigo, string descricao, int saldo)
    {
        var now = DateTime.UtcNow;
        return new Produto
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Descricao = descricao,
            Saldo = saldo,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string codigo, string descricao, int saldo)
    {
        Codigo = codigo;
        Descricao = descricao;
        Saldo = saldo;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result DescontarSaldo(int quantidade)
    {
        if (quantidade > Saldo)
            return Result.Failure(ProdutoErrors.SaldoInsuficiente(Codigo, Saldo, quantidade));

        Saldo -= quantidade;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
