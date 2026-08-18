using Korp.Estoque.Domain.Repositories;
using Korp.Estoque.Infrastructure.Persistence;
using Korp.Estoque.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EstoqueDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "korp_estoque")));

        services.AddScoped<IProdutoRepository, ProdutoRepository>();

        return services;
    }
}
