using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Infrastructure.Persistence.Repositories;

internal sealed class ProdutoRepository : IProdutoRepository
{
    private readonly EstoqueDbContext _context;

    public ProdutoRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .ToListAsync(cancellationToken);

    public async Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Produto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

    public async Task<List<Produto>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();

        // FOR UPDATE garante lock pessimista — previne race condition no desconto de saldo
        return await _context.Produtos
            .FromSqlRaw(
                $"SELECT * FROM korp_estoque.produtos WHERE id = ANY(@p0) FOR UPDATE",
                new Npgsql.NpgsqlParameter("p0", idList.ToArray()))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Produto produto, CancellationToken cancellationToken = default) =>
        await _context.Produtos.AddAsync(produto, cancellationToken);

    public Task UpdateAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        _context.Produtos.Update(produto);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        _context.Produtos.Remove(produto);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
