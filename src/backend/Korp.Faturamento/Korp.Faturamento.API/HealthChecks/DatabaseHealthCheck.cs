using Korp.Faturamento.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Korp.Faturamento.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly FaturamentoDbContext _context;

    public DatabaseHealthCheck(FaturamentoDbContext context) => _context = context;

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
