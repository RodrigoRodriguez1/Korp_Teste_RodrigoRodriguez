using System.Text.Json;
using Korp.Faturamento.Application.Common.ExternalServices;
using Korp.Faturamento.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Idempotency;

internal sealed class IdempotencyService : IIdempotencyService
{
    private readonly FaturamentoDbContext _context;

    public IdempotencyService(FaturamentoDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == key, cancellationToken);

        if (record is null)
            return default;

        return JsonSerializer.Deserialize<T>(record.ResponseBody);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        var record = new IdempotencyKey(key, json, DateTime.UtcNow);
        await _context.IdempotencyKeys.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
