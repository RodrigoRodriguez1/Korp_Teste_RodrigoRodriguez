namespace Korp.Faturamento.Infrastructure.Idempotency;

public sealed class IdempotencyKey
{
    private IdempotencyKey() { }

    public IdempotencyKey(string key, string responseBody, DateTime createdAt)
    {
        Key = key;
        ResponseBody = responseBody;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.AddHours(24);
    }

    public string Key { get; private set; } = default!;
    public string ResponseBody { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
}
