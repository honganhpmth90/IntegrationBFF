namespace IntegrationBFF.Application.Transactions.Contracts;

public sealed record TransactionDetailsResponse(
    Guid TransactionId,
    string PartnerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    DateTimeOffset Timestamp,
    string Status);
