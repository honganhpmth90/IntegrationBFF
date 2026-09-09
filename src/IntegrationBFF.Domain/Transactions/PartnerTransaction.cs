namespace IntegrationBFF.Domain.Transactions;

public sealed class PartnerTransaction
{
    private PartnerTransaction()
    {
    }

    private PartnerTransaction(
        Guid id,
        string partnerId,
        string transactionReference,
        decimal amount,
        string currency,
        DateTimeOffset transactionTimestamp,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        PartnerId = partnerId;
        TransactionReference = transactionReference;
        Amount = amount;
        Currency = currency;
        TransactionTimestamp = transactionTimestamp;
        CreatedAtUtc = createdAtUtc;
        Status = TransactionStatus.PendingPublication;
    }

    public Guid Id { get; private set; }
    public string PartnerId { get; private set; } = string.Empty;
    public string TransactionReference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTimeOffset TransactionTimestamp { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public TransactionStatus Status { get; private set; }

    public static PartnerTransaction Create(
        string partnerId,
        string transactionReference,
        decimal amount,
        string currency,
        DateTimeOffset transactionTimestamp,
        DateTimeOffset createdAtUtc) =>
        new(Guid.NewGuid(), partnerId, transactionReference, amount, currency.ToUpperInvariant(), transactionTimestamp, createdAtUtc);

    public void MarkPublished() => Status = TransactionStatus.Published;
}
