using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Application.Transactions.Contracts;
using IntegrationBFF.Core.Exceptions;
using MediatR;

namespace IntegrationBFF.Application.Transactions.Queries;

public sealed class GetPartnerTransactionQueryHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<GetPartnerTransactionQuery, TransactionDetailsResponse>
{
    public async Task<TransactionDetailsResponse> Handle(
        GetPartnerTransactionQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new TransactionNotFoundException(request.TransactionId);

        return new TransactionDetailsResponse(
            transaction.Id,
            transaction.PartnerId,
            transaction.TransactionReference,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionTimestamp,
            transaction.Status.ToString());
    }
}
