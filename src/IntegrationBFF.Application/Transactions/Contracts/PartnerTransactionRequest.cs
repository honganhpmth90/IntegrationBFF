namespace IntegrationBFF.Application.Transactions.Contracts;

public sealed record PartnerTransactionRequest(
    string PartnerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    DateTimeOffset Timestamp);
