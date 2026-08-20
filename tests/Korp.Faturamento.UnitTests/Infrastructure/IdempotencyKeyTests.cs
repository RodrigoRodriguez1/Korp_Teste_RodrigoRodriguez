using FluentAssertions;
using Korp.Faturamento.Infrastructure.Idempotency;

namespace Korp.Faturamento.UnitTests.Infrastructure;

public sealed class IdempotencyKeyTests
{
    [Fact]
    public void Constructor_ExpiresAtIgualCreatedAtMais24Horas()
    {
        var createdAt = DateTime.UtcNow;

        var key = new IdempotencyKey("minha-chave", "{}", createdAt);

        key.ExpiresAt.Should().Be(createdAt.AddHours(24));
    }

    [Fact]
    public void Constructor_ChaveNaoExpirada_ExpiresAtNoFuturo()
    {
        var key = new IdempotencyKey("chave-nova", "{}", DateTime.UtcNow);

        key.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ChaveExpirada_ExpiresAtNoPassado()
    {
        var createdAtHa25Horas = DateTime.UtcNow.AddHours(-25);

        var key = new IdempotencyKey("chave-antiga", "{}", createdAtHa25Horas);

        key.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_PropriedadesPreenchadasCorretamente()
    {
        var createdAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var key = new IdempotencyKey("teste-key", @"{""id"":1}", createdAt);

        key.Key.Should().Be("teste-key");
        key.ResponseBody.Should().Be(@"{""id"":1}");
        key.CreatedAt.Should().Be(createdAt);
        key.ExpiresAt.Should().Be(new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc));
    }
}
