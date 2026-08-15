using Korp.Faturamento.Application.Common.ExternalServices;
using Korp.Faturamento.Domain.Repositories;
using Korp.Faturamento.Infrastructure.ExternalServices;
using Korp.Faturamento.Infrastructure.Idempotency;
using Korp.Faturamento.Infrastructure.Persistence;
using Korp.Faturamento.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Korp.Faturamento.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FaturamentoDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "faturamento")));

        services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        var estoqueBaseUrl = configuration["EstoqueService:BaseUrl"]
            ?? throw new InvalidOperationException("EstoqueService:BaseUrl não configurado.");

        services
            .AddHttpClient<IEstoqueService, EstoqueHttpService>(client =>
            {
                client.BaseAddress = new Uri(estoqueBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    // 3 tentativas com backoff exponencial: 2s, 4s, 8s
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    // Abre o circuito após 5 falhas consecutivas, mantém aberto por 30s
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
}
