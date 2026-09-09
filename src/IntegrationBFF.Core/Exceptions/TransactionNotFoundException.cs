namespace IntegrationBFF.Core.Exceptions;

public sealed class TransactionNotFoundException(Guid transactionId)
    : IntegrationBffException($"Transaction '{transactionId}' was not found.");
