namespace IntegrationBFF.Domain.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(Guid id, string type, string payload, DateTimeOffset occurredAtUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    public static OutboxMessage Create(string type, string payload, DateTimeOffset occurredAtUtc) =>
        new(Guid.NewGuid(), type, payload, occurredAtUtc);

    public void MarkProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        LastError = error.Length <= 2000 ? error : error[..2000];
    }
}
