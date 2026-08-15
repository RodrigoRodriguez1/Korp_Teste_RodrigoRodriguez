using Korp.Estoque.Domain.Entities;

namespace Korp.Estoque.Domain.Repositories;

public interface IProdutoRepository
{
    Task<List<Produto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Produto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<List<Produto>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AddAsync(Produto produto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Produto produto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Produto produto, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
