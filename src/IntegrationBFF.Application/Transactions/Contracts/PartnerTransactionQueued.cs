namespace IntegrationBFF.Application.Transactions.Contracts;

public sealed record PartnerTransactionQueued(
    Guid TransactionId,
    string PartnerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    DateTimeOffset Timestamp,
    DateTimeOffset AcceptedAtUtc);
