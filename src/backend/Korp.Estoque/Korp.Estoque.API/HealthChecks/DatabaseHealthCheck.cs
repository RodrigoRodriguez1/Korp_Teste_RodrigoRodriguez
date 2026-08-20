using Korp.Estoque.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Korp.Estoque.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly EstoqueDbContext _context;

    public DatabaseHealthCheck(EstoqueDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
