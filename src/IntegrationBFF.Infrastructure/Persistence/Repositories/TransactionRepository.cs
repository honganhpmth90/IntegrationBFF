using IntegrationBFF.Application.Abstractions.Persistence;
using IntegrationBFF.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace IntegrationBFF.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository(IntegrationBffDbContext dbContext) : ITransactionRepository
{
    public Task<PartnerTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PartnerTransactions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PartnerTransaction?> GetByReferenceAsync(
        string partnerId,
        string transactionReference,
        CancellationToken cancellationToken) =>
        dbContext.PartnerTransactions.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.PartnerId == partnerId && x.TransactionReference == transactionReference,
                cancellationToken);

    public Task AddAsync(PartnerTransaction transaction, CancellationToken cancellationToken) =>
        dbContext.PartnerTransactions.AddAsync(transaction, cancellationToken).AsTask();
}
