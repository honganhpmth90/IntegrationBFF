namespace IntegrationBFF.Application.Transactions.Contracts;

public sealed record TransactionAcceptedResponse(
    Guid TransactionId,
    string Status,
    bool IsDuplicate = false);
