using IntegrationBFF.Application.Transactions.Contracts;
using MediatR;

namespace IntegrationBFF.Application.Transactions.Queries;

public sealed record GetPartnerTransactionQuery(Guid TransactionId) : IRequest<TransactionDetailsResponse>;
