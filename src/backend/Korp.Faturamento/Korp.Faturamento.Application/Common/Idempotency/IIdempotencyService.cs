namespace Korp.Faturamento.Application.Common.ExternalServices;

public interface IIdempotencyService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
}
