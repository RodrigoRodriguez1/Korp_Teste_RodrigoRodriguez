using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Persistence;

public sealed class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNota> ItensNota => Set<ItemNota>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("korp_faturamento");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FaturamentoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
