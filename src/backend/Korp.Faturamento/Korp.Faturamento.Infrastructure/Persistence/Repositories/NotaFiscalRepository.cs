using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Persistence.Repositories;

internal sealed class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly FaturamentoDbContext _context;

    public NotaFiscalRepository(FaturamentoDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotaFiscal>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.NotasFiscais
            .AsNoTracking()
            .Include(n => n.Itens)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<NotaFiscal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<int> GetNextNumeroAsync(CancellationToken cancellationToken = default)
    {
        var result = await _context.Database
            .SqlQueryRaw<long>("SELECT nextval('korp_faturamento.nota_fiscal_numero_seq') AS \"Value\"")
            .FirstAsync(cancellationToken);
        return (int)result;
    }

    public async Task AddAsync(NotaFiscal nota, CancellationToken cancellationToken = default) =>
        await _context.NotasFiscais.AddAsync(nota, cancellationToken);

    public Task UpdateAsync(NotaFiscal nota, CancellationToken cancellationToken = default)
    {
        _context.NotasFiscais.Update(nota);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
