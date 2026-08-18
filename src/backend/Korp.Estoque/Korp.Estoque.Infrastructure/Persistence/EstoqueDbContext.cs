using Korp.Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Infrastructure.Persistence;

public sealed class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("korp_estoque");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EstoqueDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
