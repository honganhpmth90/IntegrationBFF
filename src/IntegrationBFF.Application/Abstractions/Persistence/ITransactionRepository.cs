using IntegrationBFF.Domain.Transactions;

namespace IntegrationBFF.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<PartnerTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PartnerTransaction?> GetByReferenceAsync(string partnerId, string transactionReference, CancellationToken cancellationToken);
    Task AddAsync(PartnerTransaction transaction, CancellationToken cancellationToken);
}
