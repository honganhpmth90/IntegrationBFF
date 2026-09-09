using IntegrationBFF.Application.Transactions.Contracts;
using MediatR;

namespace IntegrationBFF.Application.Transactions.Commands;

public sealed record CreatePartnerTransactionCommand(
    string PartnerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    DateTimeOffset Timestamp) : IRequest<TransactionAcceptedResponse>;
