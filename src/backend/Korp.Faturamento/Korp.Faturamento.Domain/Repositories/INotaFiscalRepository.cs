using Korp.Faturamento.Domain.Entities;

namespace Korp.Faturamento.Domain.Repositories;

public interface INotaFiscalRepository
{
    Task<List<NotaFiscal>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NotaFiscal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetNextNumeroAsync(CancellationToken cancellationToken = default);
    Task AddAsync(NotaFiscal notaFiscal, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotaFiscal notaFiscal, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
